using Microsoft.AspNetCore.Mvc;
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
            var words = (from w in _context.Words
                        from y in _context.YoutubeLinks.Where(x => x.WordId == w.Id).DefaultIfEmpty()
                        group w by new { w.Id, w.Name, w.CreateTime, w.WordTypeId } into g
                        select new MainIndex
                        {
                            Id=g.Key.Id,
                            Name=g.Key.Name,
                            CreateTime=g.Key.CreateTime,
                            WordTypeId=g.Key.WordTypeId,
                            Count=g.Count()
                        }).ToList();

            //var words = _context.Words
            //.Include(w => w.WordTypes)
            //.Include(wt => wt.YoutubeLinks)
            //.ToList();

            ViewBag.WordsCount = words.Count;

            //words[0].YoutubeLink

            var wordTypes = _context.WordTypes.ToList();
            ViewBag.WordTypes = wordTypes;
            //_context.Words.Add(word);
            //_context.SaveChanges();
            //}

            return View(words);
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
            var result = _context.Words.Where(x => x.Name.StartsWith(word)).ToList();
            List<MainIndex> main = new List<MainIndex>();
            //main.Add(new MainIndex { Id=1, Name = result[0] })
            return PartialView("_tableWords", result);
        }
    }
}