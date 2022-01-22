namespace DFPay.MVC.Models.RapydModels
{
    public class RapydWebhook
    {
        public string id { get; set; }
        public string type { get; set; }
        public RapydWebhookData data { get; set; }
        public string trigger_operation_id { get; set; }
        public string status { get; set; }
        public string created_at { get; set; }
    }
}
