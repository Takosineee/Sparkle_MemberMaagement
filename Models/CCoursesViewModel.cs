namespace Gym.Models
{
    public class CCoursesViewModel
    {
        public int courseId { get; set; }
        public string? courseName { get; set; }
        public string? courseDescribe { get; set; }
        public DateTime? startReserveTime { get; set; }
        public DateTime? endReserveTime { get; set;}
        public DateTime courseStartTime { get; set; }
        public DateTime courseEndTime { get; set; } 
        public int? capacity { get; set; }
        public string? teacherName { get; set; }
        public string? status { get; set; }

    }
}
