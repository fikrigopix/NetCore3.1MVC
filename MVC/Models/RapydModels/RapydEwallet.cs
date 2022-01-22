namespace DFPay.MVC.Models.RapydModels
{
    public class RapydEwallet
    {
        public decimal amount { get; set; }
        public decimal percent { get; set; }
        public string ewallet_id { get; set; }
        public decimal refunded_amount { get; set; }
        public decimal released_amount { get; set; }
    }
}
