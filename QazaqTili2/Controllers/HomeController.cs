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
            //Word word = new Word()
            //{
            //    Name = "Барады",
            //    CreateTime=DateTime.Now
            //};

            //using (ApplicationContext db = new ApplicationContext())
            //{
                var words = _context.Words
                .Include(w => w.WordTypes)
                .ToList();

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

            Word word = new Word();
            word.Name = wordName;
            word.CreateTime = DateTime.Now;
            word.WordTypeId = wordType;
            _context.Words.Add(word);
            _context.SaveChanges();

            return Redirect("Index");
        }
    }
}