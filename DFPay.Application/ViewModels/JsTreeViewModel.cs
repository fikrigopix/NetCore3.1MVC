using System;
using System.Collections.Generic;
using System.Text;

namespace DFPay.Application.ViewModels
{
    public class JsTreeViewModel
    {
        public string id { get; set; }
        public string parent { get; set; }
        public string text { get; set; }
        public string icon { get; set; }
        public State state { get; set; }      
        public string li_attr { get; set; }
        public string a_attr { get; set; }
    }

    public class State
    {
        public bool opened { get; set; }
        public bool disabled { get; set; }
        public bool selected { get; set; }
    }
}
