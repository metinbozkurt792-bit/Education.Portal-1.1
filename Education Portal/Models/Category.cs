using System.ComponentModel.DataAnnotations;

namespace Education_Portal.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı boş geçilemez.")]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
