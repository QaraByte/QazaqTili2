using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QazaqTili2.Models;

namespace QazaqTili2.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationContext _context;

        public AnalyticsController(/*ILogger<AnalyticsController> logger,*/ ApplicationContext context)
        {
            //_logger = (ILogger<HomeController>?)logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var lastWords = _context.Words.Take(10).OrderByDescending(x => x.CreateTime).ToList();

            ViewBag.LastImages= _context.Files
                                .OrderByDescending(f => f.UploadTime)
                                .Take(5)
                                .Select(f => new {
                                    FileName = f.Name,
                                    UploadTime = f.UploadTime,
                                    WordName = _context.Words
                                        .Where(w => w.Id == f.WordId)
                                        .Select(w => w.Name)
                                        .FirstOrDefault()
                                })
                                .ToList();

            var byLetters = _context.Set<AnalytByLetters>().
                                FromSqlRaw($@"select 'Всего' as FirstLetter, count(1) as Count from Words
                                                                    union all
                                                                    select left(w.Name, 1) as FirstLetter, count(1) as Count
                                                                    from words w
                                                                    where 1 = 1
                                                                    group by LEFT(w.Name, 1)")
                                .ToList();

            ViewBag.ByLetters= byLetters;
            

            return View(lastWords);
        }
    }
}
