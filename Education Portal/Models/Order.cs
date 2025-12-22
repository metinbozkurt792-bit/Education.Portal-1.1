using System.ComponentModel.DataAnnotations.Schema;

namespace Education_Portal.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
    }
}