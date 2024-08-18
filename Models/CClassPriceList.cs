namespace Gym.Models
{
    public class CClassPriceList
    {
        public int cId { get; set; }
        public string? cCourseName { get; set; }
        public int? cClassCount { get; set; }
        public int? cPerClassPrice { get; set; }
        public int? cCoursePrice
        {
            get { return this.cPerClassPrice * this.cClassCount; }
        }
        public int? cDueDay { get; set; }
    }
}
