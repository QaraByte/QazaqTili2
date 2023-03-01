using Microsoft.AspNetCore.Mvc;

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
            return View(lastWords);
        }
    }
}
