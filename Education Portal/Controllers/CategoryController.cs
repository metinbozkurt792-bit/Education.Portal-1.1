using Education_Portal.Models;
using Education_Portal.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Education_Portal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoryController(CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public IActionResult Index()
        {
            return View(_categoryRepository.GetAll());
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(Category category)
        {
            if (!ModelState.IsValid) return View(category);
            _categoryRepository.Add(category);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Update(int id)
        {
            var category = _categoryRepository.GetById(id);
            if (category == null) return RedirectToAction(nameof(Index));
            return View(category);
        }

        [HttpPost]
        public IActionResult Update(Category category)
        {
            _categoryRepository.Update(category);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteCategory(int id)
        {
            _categoryRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}