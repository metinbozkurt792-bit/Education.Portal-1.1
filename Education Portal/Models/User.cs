using System.ComponentModel.DataAnnotations;

namespace Education_Portal.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Email { get; set; }
        [Required]
        [StringLength(50)]
        public string Password { get; set; } 
        public string Role { get; set; } = "User"; 
        public DateTime? BanEndDate { get; set; }
        [Required]
        [StringLength(50)]
        public string FullName { get; set; }

    }
}
