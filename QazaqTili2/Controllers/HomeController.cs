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
            //using (ApplicationContext db = new ApplicationContext())
            //{
            var words = _context.Words
            .Include(w => w.WordTypes)
            .Include(wt => wt.YoutubeLinks)
            //=> new {}
            .ToList();

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

        public IActionResult EditYoutubeLink(int id)
        {
            var link = _context.YoutubeLinks.Find(id);
            int wordid = link.WordId;

            return Redirect("Home/EditWord/" + wordid);
        }
    }
}