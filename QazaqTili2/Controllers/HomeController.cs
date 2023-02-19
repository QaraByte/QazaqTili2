using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QazaqTili2.Models;
using System.Diagnostics;

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

            //var words = _context.Database.SqlQuery<MainIndex>($@"with a as (
            //                                        select top {page * 20} row_number() over (order by w.id) as Row, w.Id, w.Name, w.CreateTime, w.WordTypeId, t.Name as WordTypeName, count(y.Id) as Count
            //                                        from Words w
            //                                        left join WordTypes t on w.WordTypeId=t.Id
            //                                        left join YoutubeLinks y on w.Id=y.WordId
            //                                        group by w.Id, w.Name, w.CreateTime, w.WordTypeId, t.Name
            //                                        order by w.Name)
            //                                        select * from a
            //                                        where a.Row between {range}
            //                                        order by a.Name").ToList();

            //var words = (from w in _context.Words
            //             from y in _context.YoutubeLinks.Where(x => x.WordId == w.Id).DefaultIfEmpty()
            //             from t in _context.WordTypes.Where(y => y.Id == w.WordTypeId).DefaultIfEmpty()
            //             .Where(x=>x.)
            //             group w by new NewRecord(w.Id, w.Name, w.CreateTime, w.WordTypeId, t.Name) into g
            //             select new MainIndex
            //             {
            //                 Id = g.Key.Id,
            //                 Name = g.Key.Name,
            //                 CreateTime = g.Key.CreateTime,
            //                 WordTypeId = g.Key.WordTypeId,
            //                 WordTypeName = g.Key.WordTypeName,
            //                 Count = g.Count()
            //             })
            //             //.Take(current)
            //             .ToList();

            ViewBag.SelectedPage = page;

            int wordsCount = _context.Words.Count();

            ViewBag.WordsCount = wordsCount;
            ViewBag.PagesCount = wordsCount / 20;

            var wordTypes = _context.WordTypes.ToList();
            ViewBag.WordTypes = wordTypes;

            //return PartialView("_tableWords", words);
            return words;
        }

        public IActionResult SelectPageAjax(int page = 1)
        {
            var words= SelectPage(page);

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

        public IActionResult AddWord()
        {
            var list = Request.Form.ToList();

            string wordName = list.Find(x => x.Key == "word").Value.ToString();
            int wordType = int.Parse(list.Find(x => x.Key == "word-types").Value);
            string youtubeLinkValue = list.Find(x => x.Key == "youtube").Value.ToString();

            Word word = new Word();
            word.Name = wordName;
            word.CreateTime = DateTime.Now;
            word.WordTypeId = wordType;
            _context.Words.Add(word);
            _context.SaveChanges();

            if(!string.IsNullOrWhiteSpace(youtubeLinkValue))
            {
                YoutubeLinks youtubeLinks = new YoutubeLinks();
                youtubeLinks.Url = youtubeLinkValue;
                youtubeLinks.WordId = word.Id;
                youtubeLinks.CreateTime= DateTime.Now;
                string wordTime = list.Find(x => x.Key == "wordtime").Value.ToString();
                youtubeLinks.WordTime = wordTime;
                _context.YoutubeLinks.Add(youtubeLinks);
                _context.SaveChanges();
            }

            return Redirect("Index");
        }

        public IActionResult EditWord(int id)
        {
            var word = _context.Words.FirstOrDefault(x => x.Id == id);

            var youtubeLinks = _context.YoutubeLinks.Where(x => x.WordId == id).ToList();
            ViewBag.YLinks = youtubeLinks;

            return View(word);
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
            var result = _context.Words.Where(x => x.Name.StartsWith(word)).Select(y=>new MainIndex { Id=y.Id, Name=y.Name, CreateTime=y.CreateTime}).ToList();

            //List<MainIndex> main = new List<MainIndex>();
            //main.Add(new MainIndex { Id=1, Name = result[0] })

            ViewBag.WordsCount = result.Count;

            return PartialView("_tableWords", result);
        }
    }

    internal record NewRecord(int Id, string Name, DateTime? CreateTime, int? WordTypeId, string WordTypeName);
}