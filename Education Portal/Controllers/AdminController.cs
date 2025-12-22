using Education_Portal.Hubs;
using Education_Portal.Models;
using Education_Portal.Repositories;
using Education_Portal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Education_Portal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly CourseRepository _courseRepository;
        private readonly VideoRepository _videoRepository;
        private readonly UserRepository _userRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<GeneralHub> _hubContext;

        public AdminController(
            CategoryRepository categoryRepository,
            CourseRepository courseRepository,
            VideoRepository videoRepository,
            UserRepository userRepository,
            UserManager<AppUser> userManager,
            ApplicationDbContext context,
            IHubContext<GeneralHub> hubContext)
        {
            _categoryRepository = categoryRepository;
            _courseRepository = courseRepository;
            _videoRepository = videoRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _context = context;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            ViewBag.CourseCount = _courseRepository.GetAll().Count;
            ViewBag.CategoryCount = _categoryRepository.GetAll().Count;
            ViewBag.UserCount = _userManager.Users.Count();
            return View();
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
            if (course == null) return RedirectToAction("Courses");
            ViewBag.Categories = _categoryRepository.GetAll();
            return View(course);
        }

        [HttpPost]
        public IActionResult UpdateCourse(Course course)
        {
            var existingCourse = _courseRepository.GetById(course.Id);
            if (existingCourse == null) return NotFound();

            existingCourse.Title = course.Title;
            existingCourse.Description = course.Description;
            existingCourse.CategoryId = course.CategoryId;
            existingCourse.Price = course.Price;

            _courseRepository.Update(existingCourse);
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

        public IActionResult Settings() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(string currentPassword, string newPassword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                ViewBag.Message = "Şifreniz başarıyla değiştirildi!";
                ViewBag.Type = "success";
            }
            else
            {
                ViewBag.Message = string.Join(" ", result.Errors.Select(e => e.Description));
                ViewBag.Type = "danger";
            }
            return View();
        }

        public IActionResult Users() => View(_userManager.Users.ToList());

        public async Task<IActionResult> BanUser(int id, int days)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                user.BanEndDate = DateTime.Now.AddDays(days);
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> UnbanUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                user.BanEndDate = null;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                string userName = user.UserName;
                await _userManager.DeleteAsync(user);
                var userCount = await _userManager.Users.CountAsync();
                await _hubContext.Clients.All.SendAsync("ReceiveUserCount", userCount);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"{userName} adlı üyenin hesabı silindi.");
            }
            return RedirectToAction(nameof(Users));
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
            return RedirectToAction(nameof(Comments));
        }

        public IActionResult Enrollments()
        {
            var list = new List<AdminCourseUserViewModel>();

            var orders = _context.Orders
                                 .Include(x => x.User)
                                 .Include(x => x.Course)
                                 .ToList();

            foreach (var item in orders)
            {
                list.Add(new AdminCourseUserViewModel
                {
                    StudentName = item.User?.FullName ?? "Silinmiş Üye",
                    StudentEmail = item.User?.Email,
                    CourseTitle = item.Course?.Title ?? "Silinmiş Kurs",
                    Date = item.Date,
                    Type = "Satın Alma",
                    Price = item.Price
                });
            }

            var enrollments = _context.Enrollments
                                      .Include(x => x.User)
                                      .Include(x => x.Course)
                                      .ToList();

            foreach (var item in enrollments)
            {
                list.Add(new AdminCourseUserViewModel
                {
                    StudentName = item.User?.FullName ?? "Silinmiş Üye",
                    StudentEmail = item.User?.Email,
                    CourseTitle = item.Course?.Title ?? "Silinmiş Kurs",
                    Date = item.EnrollmentDate,
                    Type = "Ücretsiz Kayıt",
                    Price = 0
                });
            }

            var sortedList = list.OrderByDescending(x => x.Date).ToList();
            return View(sortedList);
        }
    }
}