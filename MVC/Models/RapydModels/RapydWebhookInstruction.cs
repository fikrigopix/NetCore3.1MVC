using System.Collections.Generic;

namespace DFPay.MVC.Models.RapydModels
{
    public class RapydWebhookInstruction
    {
        public string name { get; set; }
        public List<RapydWebhookInstructionStep> steps { get; set; }
    }
}
