namespace Gym.Models
{
    public class COrderRequestViewModel
    {
        public int dProductId { get; set; }
        public int? dPrice { get; set; }
        public int? dClassCount { get; set; }
        public int? dTotalPrice
        {
            get { return this.dPrice * this.dClassCount; }
        }
    }
}
