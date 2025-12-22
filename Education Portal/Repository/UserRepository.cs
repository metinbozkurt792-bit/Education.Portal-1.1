using Education_Portal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Education_Portal.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public UserRepository(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<AppUser> GetByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> Add(AppUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public bool IsEmailExists(string email)
        {
            return _context.Users.Any(x => x.Email == email);
        }

        public List<AppUser> GetAll()
        {
            return _context.Users.AsNoTracking().ToList();
        }

        public AppUser GetById(int id)
        {
            return _context.Users.Find(id);
        }

        public async Task Update(AppUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public void BanUser(int id, DateTime banDate)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.BanEndDate = banDate;
                _context.SaveChanges();
            }
        }

        public void UnbanUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.BanEndDate = null;
                _context.SaveChanges();
            }
        }
    }
}