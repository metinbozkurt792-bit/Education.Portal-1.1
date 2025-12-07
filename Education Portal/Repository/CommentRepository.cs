using Education_Portal.Models;
using Microsoft.EntityFrameworkCore;

namespace Education_Portal.Repositories
{
    public class CommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(Comment comment)
        {
            _context.Comments.Add(comment);
            _context.SaveChanges();
        }
        public List<Comment> GetCommentsByCourse(int courseId)
        {
            return _context.Comments
                .Include(x => x.User) 
                .Where(x => x.CourseId == courseId)
                .OrderByDescending(x => x.Date) 
                .ToList();
        }
        public Comment GetById(int id)
        {
            return _context.Comments.Find(id);
        }

        public void Delete(Comment comment)
        {
            _context.Comments.Remove(comment);
            _context.SaveChanges();
        }
        public void Update(Comment comment)
        {
            _context.Comments.Update(comment);
            _context.SaveChanges();
        }
    }
}