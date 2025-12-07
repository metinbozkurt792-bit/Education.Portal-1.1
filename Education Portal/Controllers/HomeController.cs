using Education_Portal.Models;
using Education_Portal.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Education_Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CourseRepository _courseRepository;
        private readonly CategoryRepository _categoryRepository;

        public HomeController(ILogger<HomeController> logger, CourseRepository courseRepository, CategoryRepository categoryRepository)
        {
            _logger = logger;
            _courseRepository = courseRepository;
            _categoryRepository = categoryRepository;
        }

        public IActionResult Index()
        {
            ViewBag.Categories = _categoryRepository.GetAll();
            var courses = _courseRepository.GetAll();
            return View(courses);
        }

        public IActionResult GetCourses(int categoryId, string search)
        {
            var courses = _courseRepository.GetAll();

            if (categoryId > 0)
            {
                courses = courses.Where(c => c.CategoryId == categoryId).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                courses = courses.Where(c => c.Title.ToLower().Contains(search.ToLower())).ToList();
            }

            return PartialView("_CourseList", courses);
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