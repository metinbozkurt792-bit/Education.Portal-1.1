using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Education_Portal.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kurs başlığı zorunludur.")]
        [StringLength(100)]
        public string Title { get; set; }

        public string? Description { get; set; } 
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        public List<Video> Videos { get; set; }
    }
}