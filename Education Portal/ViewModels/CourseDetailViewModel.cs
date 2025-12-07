using System.ComponentModel.DataAnnotations; 
using Education_Portal.Models;

namespace Education_Portal.ViewModels
{
    public class CourseDetailViewModel
    {
        public CourseDetailViewModel()
        {
            Comments = new List<Comment>();
        }
        public Course Course { get; set; }
        public List<Comment> Comments { get; set; }
        public double AverageScore
        {
            get
            {
                if (Comments == null || !Comments.Any())
                    return 0;

                return Comments.Average(x => x.Rating);
            }
        }
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Lütfen bir yorum yazınız.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Yorum 5 ile 500 karakter arasında olmalıdır.")]
        [Display(Name = "Yorumunuz")]
        public string NewCommentText { get; set; }

        [Required(ErrorMessage = "Lütfen puan veriniz.")]
        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int NewRating { get; set; }
        public int CommentId { get; set; }
    }
}