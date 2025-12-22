using Education_Portal.Models;
using Education_Portal.Repositories; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Education_Portal.Controllers
{
    [Authorize] 
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Checkout(int courseId)
        {
            var course = _context.Courses.FirstOrDefault(x => x.Id == courseId);
            if (course == null) return NotFound();
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool alreadyBought = _context.Orders.Any(o => o.UserId == userId && o.CourseId == courseId);

            if (alreadyBought)
            {
                return RedirectToAction("CourseVideos", "Course", new { id = courseId });
            }

            return View(course);
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(int courseId, string cardName, string cardNumber)
        {
            var course = _context.Courses.FirstOrDefault(x => x.Id == courseId);
            if (course == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Order newOrder = new Order
            {
                CourseId = courseId,
                UserId = userId,
                Price = course.Price, 
                Date = DateTime.Now
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Tebrikler! Kursu başarıyla satın aldınız.";
            return RedirectToAction("CourseVideos", "Course", new { id = courseId });
        }
    }
}