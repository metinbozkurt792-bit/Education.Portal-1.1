using Education_Portal.Models;

namespace Education_Portal.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public User GetByEmailAndPassword(string email, string password)
        {
            return _context.Users.FirstOrDefault(x => x.Email == email && x.Password == password);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public bool IsEmailExists(string email)
        {
            return _context.Users.Any(x => x.Email == email);
        }

        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }

        public User GetById(int id)
        {
            return _context.Users.Find(id);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
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

        public User GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }
    }
}