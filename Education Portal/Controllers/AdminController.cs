using Education_Portal.Models;
using Education_Portal.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;

namespace Education_Portal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly CourseRepository _courseRepository;
        private readonly VideoRepository _videoRepository;
        private readonly UserRepository _userRepository;
        private readonly ApplicationDbContext _context;

        public AdminController(CategoryRepository categoryRepository, CourseRepository courseRepository, VideoRepository videoRepository, UserRepository userRepository, ApplicationDbContext context)
        {
            _categoryRepository = categoryRepository;
            _courseRepository = courseRepository;
            _videoRepository = videoRepository;
            _userRepository = userRepository;
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.CourseCount = _courseRepository.GetAll().Count;
            ViewBag.CategoryCount = _categoryRepository.GetAll().Count;
            ViewBag.UserCount = _userRepository.GetAll().Count;
            return View();
        }

        public IActionResult Categories()
        {
            var categories = _categoryRepository.GetAll();
            return View(categories);
        }

        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryRepository.Add(category);
                return RedirectToAction("Categories");
            }
            return View(category);
        }

        public IActionResult DeleteCategory(int id)
        {
            _categoryRepository.Delete(id);
            return RedirectToAction("Categories");
        }

        public IActionResult UpdateCategory(int id)
        {
            var category = _categoryRepository.GetById(id);
            if (category == null)
            {
                return RedirectToAction("Categories");
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult UpdateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryRepository.Update(category);
                return RedirectToAction("Categories");
            }
            return View(category);
        }

        public IActionResult Courses()
        {
            var courses = _courseRepository.GetAll();
            return View(courses);
        }

        public IActionResult AddCourse()
        {
            ViewBag.Categories = _categoryRepository.GetAll();
            return View();
        }

        [HttpPost]
        public IActionResult AddCourse(Course course)
        {
            _courseRepository.Add(course);
            return RedirectToAction("Courses");
        }

        public IActionResult UpdateCourse(int id)
        {
            var course = _courseRepository.GetById(id);
            if (course == null)
            {
                return RedirectToAction("Courses");
            }
            ViewBag.Categories = _categoryRepository.GetAll();
            return View(course);
        }

        [HttpPost]
        public IActionResult UpdateCourse(Course course)
        {
            _courseRepository.Update(course);
            return RedirectToAction("Courses");
        }

        public IActionResult DeleteCourse(int id)
        {
            _courseRepository.Delete(id);
            return RedirectToAction("Courses");
        }

        public IActionResult CourseVideos(int id)
        {
            var videos = _videoRepository.GetVideosByCourseId(id);
            ViewBag.CourseId = id;
            return View(videos);
        }

        public IActionResult AddVideo(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddVideo(Video video, IFormFile videoFile)
        {
            if (videoFile != null)
            {
                if (_videoRepository.IsOrderExists(video.CourseId, video.VideoOrder))
                {
                    ViewBag.Error = "Bu sıra numarası zaten dolu! Lütfen başka bir sıra girin.";
                    ViewBag.CourseId = video.CourseId;
                    return View(video);
                }
                string extension = Path.GetExtension(videoFile.FileName);
                string videoName = Guid.NewGuid() + extension;
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos", videoName);

                if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos")))
                {
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos"));
                }

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await videoFile.CopyToAsync(stream);
                }
                video.Url = "/videos/" + videoName;
            }
            _videoRepository.Add(video);
            return RedirectToAction("CourseVideos", new { id = video.CourseId });
        }

        public IActionResult DeleteVideo(int id)
        {
            var video = _videoRepository.GetById(id);
            if (video != null)
            {
                if (!string.IsNullOrEmpty(video.Url))
                {
                    string webRootPath = Directory.GetCurrentDirectory();
                    string filePath = Path.Combine(webRootPath, "wwwroot", video.Url.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                int courseId = video.CourseId;
                _videoRepository.Delete(id);
                return RedirectToAction("CourseVideos", new { id = courseId });
            }
            return RedirectToAction("Courses");
        }

        public IActionResult Settings()
        {
            return View();
        }

        [HttpPost]
        [HttpPost]
        public IActionResult Settings(string currentPassword, string newPassword)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = 0;
            if (!string.IsNullOrEmpty(userIdString))
            {
                int.TryParse(userIdString, out userId);
            }
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = _userRepository.GetById(userId);
            if (user == null) return NotFound();

            if (user.Password == currentPassword)
            {
                user.Password = newPassword;
                _userRepository.Update(user);
                ViewBag.Message = "Şifreniz başarıyla değiştirildi!";
                ViewBag.Type = "success";
            }
            else
            {
                ViewBag.Message = "Mevcut şifreniz yanlış, lütfen tekrar deneyin.";
                ViewBag.Type = "danger";
            }
            return View();
        }

        public IActionResult BanUser(int id, int days)
        {
            DateTime banUntil = DateTime.Now.AddDays(days);
            _userRepository.BanUser(id, banUntil);
            return RedirectToAction("Users");
        }

        public IActionResult UnbanUser(int id)
        {
            _userRepository.UnbanUser(id);
            return RedirectToAction("Users");
        }

        public IActionResult Users()
        {
            var users = _userRepository.GetAll();
            return View(users);
        }

        [HttpGet]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Comments()
        {
            var comments = _context.Comments
                .Include(c => c.User)
                .Include(c => c.Video)
                .OrderByDescending(c => c.Date)
                .ToList();
            return View(comments);
        }

        [HttpGet]
        public IActionResult DeleteComment(int id)
        {
            var comment = _context.Comments.Find(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                _context.SaveChanges();
            }
            return RedirectToAction("Comments");
        }
    }
}