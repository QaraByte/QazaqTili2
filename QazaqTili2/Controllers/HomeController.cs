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
            Word word = new Word()
            {
                Name = "Барады"
            };

            //using (ApplicationContext db = new ApplicationContext())
            //{
            _context.Words.Add(word);
            _context.SaveChanges();
            //}

            return View();
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
    }
}