using System;

namespace Education_Portal.ViewModels
{
    public class AdminCourseUserViewModel
    {
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string CourseTitle { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
    }
}