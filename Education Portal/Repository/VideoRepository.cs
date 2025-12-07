using Education_Portal.Models;

namespace Education_Portal.Repositories
{
    public class VideoRepository
    {
        private readonly ApplicationDbContext _context;

        public VideoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(Video video)
        {
            _context.Videos.Add(video);
            _context.SaveChanges();
        }
        public List<Video> GetVideosByCourseId(int courseId)
        {
            return _context.Videos
                .Where(x => x.CourseId == courseId)
                .OrderBy(x => x.VideoOrder) 
                .ToList();
        }
        public Video GetById(int id)
        {
            return _context.Videos.Find(id);
        }
        public void Delete(int id)
        {
            var video = _context.Videos.Find(id);
            if (video != null)
            {
                _context.Videos.Remove(video);
                _context.SaveChanges();
            }
        }
        public bool IsOrderExists(int courseId, int videoOrder)
        {
            return _context.Videos.Any(x => x.CourseId == courseId && x.VideoOrder == videoOrder);
        }
    }
}