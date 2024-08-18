namespace Gym.Models
{
    public class COrderRequest
    {
        public List<int> oClassId { get; set; } = new List<int>();
        public List<COrderRequestViewModel> oOrderVM { get; set; }
        public int? oTotalClassCount
        {
            get { return this.oOrderVM.Sum(d => d.dClassCount); }
        }
        public int? oTotalPrice
        {
            get { return this.oOrderVM.Sum(d => d.dTotalPrice); }
        }
    }
}
