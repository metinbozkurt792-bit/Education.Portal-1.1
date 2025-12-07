using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Education_Portal.Models
{
    public class Video
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Title { get; set; } 
        public string? Url { get; set; } 
        [Range(1, 1000, ErrorMessage = "Sıra numarası 1'den küçük olamaz!")]
        public int VideoOrder { get; set; } 
        public int CourseId { get; set; } 
        [ForeignKey("CourseId")]
        public Course Course { get; set; }
    }
}