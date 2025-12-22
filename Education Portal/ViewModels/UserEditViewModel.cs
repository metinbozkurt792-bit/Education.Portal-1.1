using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Education_Portal.ViewModels
{
    public class UserEditViewModel
    {
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Display(Name = "E-Posta (Değiştirilemez)")]
        public string Email { get; set; }

        [Display(Name = "Şehir")]
        public string? City { get; set; } 
        public string? CurrentImage { get; set; }

        [Display(Name = "Profil Resmi")]
        public IFormFile? ProfilePicture { get; set; }
    }
}