using System.Collections.Generic;

namespace DFPay.MVC.Models.RapydModels
{
    public class RapydWebhookData
    {
        public string id { get; set; }
        public bool paid { get; set; }
        public string order { get; set; }
        public decimal amount { get; set; }
        public string status { get; set; }
        public string dispute { get; set; }
        public string fx_rate { get; set; }
        public string invoice { get; set; }
        public string outcome { get; set; }
        public string paid_at { get; set; }
        public string refunds { get; set; }
        public bool captured { get; set; }
        public List<RapydEwallet> ewallets { get; set; }
        public RapydMetadata metadata { get; set; }
        public bool refunded { get; set; }
        public int created_at { get; set; }
        public bool is_partial { get; set; }
        public string description { get; set; }
        public string country_code { get; set; }
        public string failure_code { get; set; }
        public List<RapydWebhookInstruction> instructions { get; set; }
        public decimal payment_fees { get; set; }
        public string redirect_url { get; set; }
        public string currency_code { get; set; }
        public string group_payment { get; set; }
        public string receipt_email { get; set; }
        public RapydWebhookTextualCodes textual_codes { get; set; }
        public string customer_token { get; set; }
        public string payment_method { get; set; }
        public string receipt_number { get; set; }
        public string transaction_id { get; set; }
        public string failure_message { get; set; }
        public decimal original_amount { get; set; }
        public decimal refunded_amount { get; set; }
        public string error_payment_url { get; set; }
        public string payment_method_type { get; set; }
        public string complete_payment_url { get; set; }
        public string statement_descriptor { get; set; }
        public string merchant_reference_id { get; set; }
        public decimal merchant_requested_amount { get; set; }
        public string merchant_requested_currency { get; set; }
        public string payment_method_type_category { get; set; }
    }
}
