using Education_Portal.Models;
using Education_Portal.Repositories;
using Education_Portal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IO;

namespace Education_Portal.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly CourseRepository _courseRepository;
        private readonly CommentRepository _commentRepository;
        private readonly UserRepository _userRepository;
        private readonly VideoRepository _videoRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CourseController(CourseRepository courseRepository,
                               CommentRepository commentRepository,
                               UserRepository userRepository,
                               VideoRepository videoRepository,
                               SignInManager<AppUser> signInManager,
                               UserManager<AppUser> userManager,
                               ApplicationDbContext context,
                               IWebHostEnvironment webHostEnvironment)
        {
            _courseRepository = courseRepository;
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _videoRepository = videoRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var courses = _courseRepository.GetAll();
            return View(courses);
        }

        public IActionResult Details(int id)
        {
            var course = _courseRepository.GetCourseWithVideos(id);
            if (course == null) return NotFound();

            var comments = _commentRepository.GetCommentsByCourse(id);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            bool isOrdered = userId != 0 && _context.Orders.Any(o => o.CourseId == id && o.UserId == userId);
            bool isEnrolled = userId != 0 && _context.Enrollments.Any(e => e.CourseId == id && e.UserId == userId);

            ViewBag.IsOwner = isOrdered || isEnrolled;

            var viewModel = new CourseDetailViewModel
            {
                Course = course,
                Comments = comments,
                CourseId = id,
                NewRating = 5
            };

            return View(viewModel);
        }

        public IActionResult CourseVideos(int id)
        {
            var course = _context.Courses.FirstOrDefault(x => x.Id == id);
            if (course == null) return NotFound();

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            bool hasOrder = _context.Orders.Any(o => o.CourseId == id && o.UserId == userId);
            bool hasEnrollment = _context.Enrollments.Any(e => e.CourseId == id && e.UserId == userId);

            if (!hasOrder && !hasEnrollment)
            {
                TempData["Error"] = "Bu dersi izlemek için önce satın almalı veya kayıt olmalısınız.";
                return RedirectToAction("Details", new { id = id });
            }

            return RedirectToAction("Details", new { id = id });
        }

        public IActionResult MyCourses()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            var orderedCourseIds = _context.Orders
                                    .Where(o => o.UserId == userId)
                                    .Select(o => o.CourseId)
                                    .ToList();

            var enrolledCourseIds = _context.Enrollments
                                     .Where(e => e.UserId == userId)
                                     .Select(e => e.CourseId)
                                     .ToList();

            var allCourseIds = orderedCourseIds.Union(enrolledCourseIds).ToList();

            var myCourses = _context.Courses
                                    .Include(c => c.Category)
                                    .Where(c => allCourseIds.Contains(c.Id))
                                    .ToList();

            return View(myCourses);
        }

        [HttpPost]
        public IActionResult AddComment(CourseDetailViewModel model)
        {
            ModelState.Remove("Course");
            ModelState.Remove("Comments");

            int userId = 0;
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userIdString))
            {
                int.TryParse(userIdString, out userId);
            }

            if (userId == 0)
            {
                TempData["ErrorMessage"] = "Oturum süreniz dolmuş veya kimlik okunamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Details", new { id = model.CourseId });
            }

            bool isOrdered = _context.Orders.Any(o => o.CourseId == model.CourseId && o.UserId == userId);
            bool isEnrolled = _context.Enrollments.Any(e => e.CourseId == model.CourseId && e.UserId == userId);
            bool isOwner = isOrdered || isEnrolled;

            if (!User.IsInRole("Admin") && !isOwner)
            {
                TempData["ErrorMessage"] = "Yorum yapabilmek için bu kursa kayıtlı olmalısınız.";
                return RedirectToAction("Details", new { id = model.CourseId });
            }

            if (ModelState.IsValid)
            {
                var newComment = new Comment
                {
                    CourseId = model.CourseId,
                    Rating = model.NewRating,
                    Content = model.NewCommentText,
                    Date = DateTime.Now,
                    UserId = userId
                };

                try
                {
                    _commentRepository.Add(newComment);
                    TempData["SuccessMessage"] = "Yorum başarıyla eklendi!";
                    return RedirectToAction("Details", new { id = model.CourseId });
                }
                catch (Exception ex)
                {
                    var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    TempData["ErrorMessage"] = "Veritabanı Detaylı Hata: " + innerMessage;
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = "Kayıt Başarısız: " + string.Join(" | ", errors);
            }

            var course = _courseRepository.GetCourseWithVideos(model.CourseId);
            if (course == null) return NotFound();

            model.Course = course;
            model.Comments = _commentRepository.GetCommentsByCourse(model.CourseId);

            return View("Details", model);
        }

        public IActionResult Watch(int id)
        {
            var video = _videoRepository.GetById(id);
            if (video == null)
            {
                return NotFound();
            }
            return View(video);
        }

        [Authorize]
        public IActionResult DeleteComment(int id)
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null)
            {
                TempData["ErrorMessage"] = "Silinecek yorum bulunamadı.";
                return RedirectToAction("Index");
            }

            var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = 0;
            int.TryParse(currentUserIdString, out currentUserId);

            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin && currentUserId != comment.UserId)
            {
                TempData["ErrorMessage"] = "Bu yorumu silme yetkiniz yok. (Sahibi siz değilsiniz.)";
                return RedirectToAction("Details", new { id = comment.CourseId });
            }

            _commentRepository.Delete(comment);
            TempData["SuccessMessage"] = "Yorum başarıyla silindi.";

            return RedirectToAction("Details", new { id = comment.CourseId });
        }

        [Authorize]
        [HttpGet]
        public IActionResult EditComment(int id)
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null) return NotFound();

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int currentUserId);
            bool canEdit = User.IsInRole("Admin") || (currentUserId != 0 && currentUserId == comment.UserId);

            if (!canEdit)
            {
                TempData["ErrorMessage"] = "Bu yorumu düzenleme yetkiniz yok.";
                return RedirectToAction("Details", new { id = comment.CourseId });
            }

            return View(comment);
        }

        [HttpPost]
        public IActionResult EditComment(Comment updatedComment)
        {
            var existingComment = _commentRepository.GetById(updatedComment.Id);
            if (existingComment == null) return NotFound();

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int currentUserId);
            bool canEdit = User.IsInRole("Admin") || (currentUserId != 0 && currentUserId == existingComment.UserId);

            if (!canEdit)
            {
                return Unauthorized();
            }

            existingComment.Content = updatedComment.Content;
            existingComment.Rating = updatedComment.Rating;
            existingComment.Date = DateTime.Now;

            _commentRepository.Update(existingComment);

            TempData["SuccessMessage"] = "Yorum güncellendi!";
            return RedirectToAction("Details", new { id = existingComment.CourseId });
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi.";
                return RedirectToAction("ChangePassword", "Course");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = new UserEditViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                CurrentImage = user.ProfileImage
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserEditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                if (model.ProfilePicture != null)
                {
                    string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "profiles");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string extension = Path.GetExtension(model.ProfilePicture.FileName);
                    string newImageName = Guid.NewGuid().ToString() + extension;
                    string filePath = Path.Combine(folderPath, newImageName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(stream);
                    }

                    user.ProfileImage = newImageName;
                }

                user.FullName = model.FullName;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi.";
                    return RedirectToAction("EditProfile");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            else
            {
                model.CurrentImage = user.ProfileImage;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Join(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                var exists = _context.Enrollments.Any(x => x.UserId == user.Id && x.CourseId == id);
                if (exists) return RedirectToAction("MyCourses");

                var enrollment = new Enrollment
                {
                    UserId = user.Id,
                    CourseId = id,
                    EnrollmentDate = DateTime.Now
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kayıt Başarılı!";
                return RedirectToAction("MyCourses");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kayıt Hatası: " + ex.Message + (ex.InnerException != null ? " | " + ex.InnerException.Message : "");
                return RedirectToAction("Details", new { id = id });
            }
        }
    }
}