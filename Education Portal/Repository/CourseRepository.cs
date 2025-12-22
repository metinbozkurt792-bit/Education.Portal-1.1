using Education_Portal.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Education_Portal.Repositories
{
    public class CourseRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Course> GetAll()
        {
            return _context.Courses
                           .Include(c => c.Category)
                           .Include(c => c.Comments) 
                           .ToList();
        }

        public void Add(Course course)
        {
            _context.Courses.Add(course);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var course = _context.Courses.Find(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                _context.SaveChanges();
            }
        }

        public Course GetById(int id)
        {
            return _context.Courses.Find(id);
        }

        public void Update(Course course)
        {
            _context.Courses.Update(course);
            _context.SaveChanges();
        }

        public Course GetCourseWithVideos(int id)
        {
            return _context.Courses
                .Include(c => c.Videos)
                .Include(c => c.Category)
                .FirstOrDefault(c => c.Id == id);
        }
    }
}