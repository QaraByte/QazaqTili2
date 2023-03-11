using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QazaqTili2.Models;
using System.Diagnostics;
using System.Text;

namespace QazaqTili2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var words = SelectPage();

            return View(words);
        }

        public List<MainIndex> SelectPage(int page = 1)
        {
            string range = $"{page * 20 - 19} and {page * 20}";

            var words = _context.Set<MainIndex>().
                                FromSqlRaw($@" with a as (
                                                 select top {page * 20} row_number() over (order by w.Name) as Row, isnull(w.Id,0) as Id, isnull(w.Name,'name') as Name
                                                , isnull(w.CreateTime, CONVERT(DATETIME2, '1970-01-01 0:00:00', 23)) as CreateTime
                                                , isnull(w.WordTypeId,0) as WordTypeId, isnull(t.Name,'-') as WordTypeName, count(y.Id) as Count
                                                    from Words w
                                                    left join WordTypes t on w.WordTypeId=t.Id
                                                    left join YoutubeLinks y on w.Id=y.WordId
                                                    group by w.Id, w.Name, w.CreateTime, w.WordTypeId, t.Name
                                                    order by w.Name
                                                     )
                                                    select * from a
                                                    where a.row between {range}
                                                    order by a.Name
                                                    --new SqlParameter('@page', page*20)
                                                    ")
                                .ToList()
                                .Select(x => new MainIndex
                                {
                                    Row = x.Row,
                                    Id = x.Id,
                                    Name = x.Name,
                                    CreateTime = x.CreateTime,// != null ? DateTime.SpecifyKind(x.CreateTime.Value, DateTimeKind.Utc) : (DateTime?)null,
                                    WordTypeId = x.WordTypeId,// != null ? x.WordTypeId : 0,
                                    WordTypeName = x.WordTypeName,
                                    Count = x.Count
                                })
                                .ToList();

            ViewBag.SelectedPage = page;

            int wordsCount = _context.Words.Count();

            ViewBag.WordsCount = (double)wordsCount;
            int v = ((double)wordsCount / 20 % 1) == 0 ? wordsCount / 20 : (wordsCount / 20) + 1;
            ViewBag.PagesCount = v;

            var wordTypes = _context.WordTypes.ToList();
            ViewBag.WordTypes = wordTypes;

            //return PartialView("_tableWords", words);
            return words;
        }

        public IActionResult SelectPageAjax(int page = 1)
        {
            var words = SelectPage(page);

            return PartialView("_tableWords", words);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddWord([FromBody] WordFromModal model)
        {
            //var list = Request.Form.ToList();
            if (model == null)
                return BadRequest("Заполните форму.");
            if (model.Name == null)
                return BadRequest("Слово не может быть пустым.");

            WordFromModal word = new WordFromModal();
            word.Name = model.Name;
            word.WordTypeId = model.WordTypeId;

            if (IsWordExists(word.Name))
            {
                string errorMessage = "Слово \"" + word.Name + "\" уже есть в словаре.";
                return BadRequest(errorMessage);
                //byte[] bytes = Encoding.UTF8.GetBytes(errorMessage);
                ////return new JsonResult(Encoding.UTF8.GetString(bytes));
                //return new ContentResult
                //{
                //    ContentType = "text/plain; charset=utf-8",
                //    Content = Encoding.UTF8.GetString(bytes)
                //};
            }


            //int wordType = int.Parse(list.Find(x => x.Key == "WordTypeId").Value);
            //string youtubeLinkValue = list.Find(x => x.Key == "Youtube").Value.ToString();

            //Word word = new Word();
            //word.Name = wordName;
            //word.CreateTime = DateTime.Now;
            Word word1 = new Word()
            {
                Name = word.Name,
                CreateTime = DateTime.Now,
                WordTypeId = word.WordTypeId
            };
            //word.WordTypeId = wordType;
            
            try
            {
                _context.Words.Add(word1);
                _context.SaveChanges();
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            if (!string.IsNullOrWhiteSpace(model.Youtube))
            {
                YoutubeLinks youtubeLinks = new YoutubeLinks();
                youtubeLinks.Url = model.Youtube;
                youtubeLinks.WordId = word1.Id;
                youtubeLinks.CreateTime = DateTime.Now;
                //string wordTime = list.Find(x => x.Key == "wordtime").Value.ToString();
                //string wordTime = model.WordTime;
                youtubeLinks.WordTime = model.WordTime;
                _context.YoutubeLinks.Add(youtubeLinks);
                _context.SaveChanges();
            }

            return Redirect("Index");
        }

        [Authorize]
        public IActionResult EditWord(int id)
        {
            var word = _context.Words.FirstOrDefault(x => x.Id == id);

            var youtubeLinks = _context.YoutubeLinks.Where(x => x.WordId == id).ToList();
            ViewBag.YLinks = youtubeLinks;

            var files = _context.Files.Where(x => x.WordId == id).ToList();
            ViewBag.Files = files;

            var imageLinks = (from l in _context.ImageLinks
                             join w in _context.Words on l.WordId equals w.Id
                         join w1 in _context.Words on l.ParentWordId equals w1.Id
                         where l.WordId == id
                              orderby l.Id
                         select new
                         {
                             l.Id,
                             l.ParentWordId,
                             ParentWord = w1.Name,
                             l.WordId,
                             Name = w.Name,
                             l.CreateTime
                         }).ToList();

            ViewBag.ImageLinks = imageLinks;

            //string directoryPath = Request.Scheme + "://" + Request.Host + "/" + files[0].Path;
            //if (Directory.Exists(directoryPath))
            //{
            //    Console.WriteLine("Каталог существует");
            //}
            //else
            //{
            //    Console.WriteLine("Каталог не существует");
            //}

            ViewData["wordId"] = word.Id;

            ModelForDropDownWords model = new ModelForDropDownWords();
            //model.SelectedOption = 7;
            model.Options = _context.Words.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Value = x.Id.ToString(), Text = x.Name }).OrderBy(c => c.Text).ToList();

            EditWordModel editWordModel = new EditWordModel()
            {
                modelForDropDownWords = model,
                Word = word
            };

            return View(editWordModel);
        }

        [HttpPost]
        public IActionResult EditYoutubeLink(int id)
        {
            YoutubeLinks link = null;
            int wordid = 0;
            if (id == 0)
            {
                link = new YoutubeLinks();
                var valueWordId = Request.Form.ToDictionary(x => x.Key, x => x.Value).ToList().Find(w => w.Key == "wordid");
                wordid = int.Parse(valueWordId.Value);
                link.WordId = wordid;
            }
            else
            {
                link = _context.YoutubeLinks.Find(id);
                wordid = link.WordId;
            }

            var dict = Request.Form.ToDictionary(x => x.Key, x => x.Value);

            foreach (var item in dict)
            {
                if (item.Key == "youtube")
                    link.Url = item.Value;
                if (item.Key == "wordtime")
                    link.WordTime = item.Value;
                if (item.Key == "name" && !string.IsNullOrEmpty(item.Value))
                    link.Name = item.Value;
            }

            if (id == 0)
            {
                _context.YoutubeLinks.Add(link);
            }
            else
                _context.YoutubeLinks.Update(link);
            _context.SaveChanges();

            return Redirect("/Home/EditWord/" + wordid);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWordName([FromForm] Word word)
        {
            //var dict = Request.Form.ToDictionary(x => x.Key, x => x.Value);

            //_context.Words.Update(word);

            _context.Attach(word);
            var entry = _context.Entry(word);
            entry.State = EntityState.Modified;
            entry.Property(e => e.CreateTime).IsModified = false;
            await _context.SaveChangesAsync();

            return Redirect("/Home/EditWord/" + word.Id);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveLink(int id)
        {
            var link = _context.YoutubeLinks.Find(id);
            if (link == null)
                return StatusCode(400);
            _context.Remove(link);
            await _context.SaveChangesAsync();
            int wordId = link.WordId;
            return Redirect("/Home/EditWord/" + wordId);
        }

        public IActionResult SearchWord(string word)
        {
            //Wo
            var result = _context.Words.Where(x => x.Name.StartsWith(word)).Select(y => new MainIndex { Id = y.Id, Name = y.Name, CreateTime = y.CreateTime }).ToList();

            //List<MainIndex> main = new List<MainIndex>();
            //main.Add(new MainIndex { Id=1, Name = result[0] })

            ViewBag.WordsCount = result.Count;

            ViewBag.PagesCount=result.Count/20;
            ViewBag.SelectedPage = 1;

            return PartialView("_tableWords", result);
        }

        private bool IsWordExists(string word)
        {
            // Проверяем, есть ли слово в словаре
            //var searchWord = _context.Words.Where(x => x.Name.ToUpper() == word.Name.ToUpper()).FirstOrDefault();
            return _context.Words.Any(w => w.Name == word);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(IFormFile fileUpload, int recordId)
        {
            if (fileUpload != null && fileUpload.Length > 0)
            {
                var fileName = Path.GetFileName(fileUpload.FileName);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileUpload.FileName);

                string ext = Path.GetExtension(fileName);
                if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
                    return BadRequest("Необходимо загружать изображения только с расширением: .png, .jpg, .jpeg. Сейчас: " + ext);

                var files = HasFiles(recordId);

                int numberOfFile = 1;
                if(files.Count>0)
                {
                    foreach (var file in files)
                    {
                        if (file == files.Last())
                        {
                            string[] name = file.Path.Split("_");
                            if (name.Length > 2)
                            {
                                //int.TryParse(name[name.Length - 1], out int n);
                                string[] ext1 = name[name.Length - 1].Split('.');
                                numberOfFile = numberOfFile + int.Parse(ext1[0]);
                            }
                        }
                    }
                }

                var filePath = Path.Combine("uploads", $"{recordId}_{fileNameWithoutExtension}_{numberOfFile}{ext}");

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileUpload.CopyToAsync(stream);
                }

                var fileModel = new FileModel
                {
                    Name = fileName,
                    ContentType = fileUpload.ContentType,
                    Path = filePath,
                    Extension = ext,
                    WordId = recordId,
                    UploadTime = DateTime.Now
                };

                _context.Files.Add(fileModel);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public List<FileModel> HasFiles(int recordId)
        {
            return _context.Files.Where(f => f.WordId == recordId).ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkImageWord([FromBody] LinkImageFromModal model)
        {
            var word = await _context.Words.Where(x => x.Id == model.WordId).FirstOrDefaultAsync();

            if (word == null)
                return BadRequest("Слово не найдено.");

            ImageLinks imageLinks = new ImageLinks();
            imageLinks.WordId = model.WordId;
            imageLinks.ParentWordId = model.ParentWordId;
            imageLinks.CreateTime = DateTime.Now;
            _context.ImageLinks.Add(imageLinks);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return StatusCode(418);
            }
            return Redirect("/Home/EditWord/" + model.WordId);
        }

        public async Task<string> GetImageInfo(int id)
        {
            var img = await _context.Files.FirstOrDefaultAsync(x => x.WordId == id);
            if(img==null)
                return "400";
            return img.Name;
        }

        public string GetPages(int countPages, int selected)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < countPages; i++)
            {
                if (i == selected)
                    sb.Append($"<div class=\"page selected-page\" data-page-number=\"{selected}\">{selected}</div>");
                else
                    sb.Append($"<div class=\"page\" data-page-number=\"{i}\">{i}</div>");

                if(i>2)
                {

                    if (selected == 3)
                    {
                        sb.Append($"<div class=\"page\" data-page-number=\"{(selected + 1)}\">{(selected + 1)}</div>");
                        sb.Append($"<div>...</div>");
                        selected = 0;
                        i = countPages - 4;
                    }
                    else if (selected == 4)
                    {
                        sb.Append($"<div class=\"page selected-page\" data-page-number=\"{selected}\">{selected}</div>");
                        sb.Append($"<div class=\"page\" data-page-number=\"{(selected + 1)}\">{(selected + 1)}</div>");
                        sb.Append($"<div>...</div>");
                        selected = 0;
                        i = countPages - 4;
                    }
                    else if (selected == 5)
                    {
                        sb.Append($"<div class=\"page\" data-page-number=\"{(selected - 1)}\">{(selected - 1)}</div>");
                        sb.Append($"<div class=\"page selected-page\" data-page-number=\"{selected}\">{selected}</div>");
                        //sb.Append($"<div class=\"page\" data-page-number=\"{(selected - 1)}\">{(selected - 1)}</div>");
                        sb.Append($"<div class=\"page\" data-page-number=\"{(selected + 1)}\">{(selected + 1)}</div>");
                        sb.Append($"<div>...</div>");
                        selected = 0;
                        i = countPages - 4;
                    }
                    else if (selected > 5 && selected < countPages - 4)
                    {
                        //if (selected > 6)
                            sb.Append($"<div>...</div>");
                        sb.Append($"<div class=\"page\" data-page-number=\"{(selected - 1)}\">{(selected - 1)}</div>");
                        sb.Append($"<div class=\"page selected-page\" data-page-number=\"{selected}\">{selected}</div>");
                        sb.Append($"<div class=\"page\" data-page-number=\"{(selected + 1)}\">{(selected + 1)}</div>");
                        sb.Append($"<div>...</div>");
                        selected = 0;
                        i = countPages - 4;
                    }
                }

                if (i > 2 && i < countPages - 4)
                {
                    sb.Append($"<div>...</div>");
                    i = countPages - 4;
                }

                //if (selected - 1 > 0)
                //    sb.Append($"<div class=\"page\" data-page-number=\"{(selected - 1)}\">{(selected - 1)}</div>");
                //sb.Append($"<div class=\"page selected-page\" data-page-number=\"{selected}\">{selected}</div>");
                //sb.Append($"<div class=\"page\" data-page-number=\"{(selected + 1)}\">{(selected + 1)}</div>");

                //i = countPages - 4;
                //if (i == selected)
                //    sb.Append($"<div class=\"page selected-page\" data-page-number=\"{selected}\">{selected}</div>");
                //else if (i < 4 || i > countPages - 4)
                //    sb.Append($"<div class=\"page\" data-page-number=\"{i}\">{i}</div>");
                //else if (selected > 2 && selected < countPages - 2)
                //{
                //    if (i > 3 && i < countPages - 3)
                //    {
                //        sb.Append($"<div class=\"page\" data-page-number=\"{(selected - 1)}\">{(selected - 1)}</div>");
                //        //sb.Append($"<div class=\"page selected-page\" data-page-number=\"{selected}\">{selected}</div>");
                //        sb.Append($"<div class=\"page\" data-page-number=\"{(selected + 1)}\">{(selected + 1)}</div>");
                //    }
                //    if (selected < countPages - 4)
                //        i = countPages - 4;
                //}

                //    sb.Append($"<div class=\"page\" data-page-number=\"{i}\">{i}</div>");

                //sb.Append($"<div class=\"page selected-page\" data-page-number=\"{selected}\">{selected}</div>");
                //sb.Append($"<div class=\"page\" data-page-number=\"{(selected + 1)}\">{(selected + 1)}</div>");

                //if(i==selected)
                //{
                //    if(i>4 && i<countPages-3)
                //    {
                //        if (i == selected)
                //        {
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(selected - 1)}\">{(selected - 1)}</div>");
                //            sb.Append($"<div class=\"page selected-page\" data-page-number=\"{selected}\">{selected}</div>");
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(selected + 1)}\">{(selected + 1)}</div>");
                //            i = countPages - 4;
                //        }
                //    }
                //    else if(i<5)
                //    {
                //        if (i == 1)
                //        {
                //            sb.Append($"<div class=\"page selected-page\" data-page-number=\"{i}\">{i}</div>");
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(i + 1)}\">{(i + 1)}</div>");
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(i + 2)}\">{(i + 2)}</div>");
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(i + 3)}\">{(i + 3)}</div>");
                //        }
                //        else if (i == 2)
                //        {
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(i - 1)}\">{(i - 1)}</div>");
                //            sb.Append($"<div class=\"page selected-page\" data-page-number=\"{i}\">{i}</div>");
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(i + 1)}\">{(i + 1)}</div>");
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(i + 2)}\">{(i + 2)}</div>");
                //        }
                //        else if (i == 3)
                //        {
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(i - 1)}\">{(i - 1)}</div>");
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(i - 2)}\">{(i - 2)}</div>");
                //            sb.Append($"<div class=\"page selected-page\" data-page-number=\"{i}\">{i}</div>");
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(i + 1)}\">{(i + 1)}</div>");
                //        }
                //        else if (i == 4)
                //        {
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(i - 1)}\">{(i - 1)}</div>");
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(i - 2)}\">{(i - 2)}</div>");
                //            sb.Append($"<div class=\"page selected-page\" data-page-number=\"{i}\">{i}</div>");
                //            sb.Append($"<div class=\"page\" data-page-number=\"{(i + 1)}\">{(i + 1)}</div>");
                //            i = countPages - 4;

                //if (i < selected - 1)
                //    i = selected - 1;
            }
            return sb.ToString();
        }
    }

    internal record NewRecord(int Id, string Name, DateTime? CreateTime, int? WordTypeId, string WordTypeName);

    public class EditWordModel
    {
        public Word Word { get; set; }
        public ModelForDropDownWords modelForDropDownWords { get; set; }
    }
}