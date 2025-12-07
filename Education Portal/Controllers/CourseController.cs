using Education_Portal.Models;
using Education_Portal.Repositories;
using Education_Portal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace Education_Portal.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly CourseRepository _courseRepository;
        private readonly CommentRepository _commentRepository;
        private readonly UserRepository _userRepository;
        private readonly VideoRepository _videoRepository;

        public CourseController(CourseRepository courseRepository, CommentRepository commentRepository, UserRepository userRepository, VideoRepository videoRepository)
        {
            _courseRepository = courseRepository;
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _videoRepository = videoRepository;
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

            var viewModel = new CourseDetailViewModel
            {
                Course = course,
                Comments = comments,
                CourseId = id,
                NewRating = 5
            };

            return View(viewModel);
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
                    TempData["ErrorMessage"] = "Veritabanı Hatası: " + ex.Message;
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
    }
}