using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFPay.Application.ViewModels
{
    public class InvoiceViewModel
    {
        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Invoice #")]
        [StringLength(15)]
        public string InvoiceNo { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Client Name")]
        [StringLength(50)]
        public string ClientName { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Invalid Email Address.")]
        [Display(Name = "Client Email")]
        [StringLength(200)]
        public string ClientEmail { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Amount")]
        [Range(0.01, 999999999999999.99, ErrorMessage = "{0} must be greater than {1} and less than {2}")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        //[Required(ErrorMessage = "The {0} field is required.")]
        //[Display(Name = "Description")]
        //[StringLength(200, ErrorMessage = "{0} length can not be more than {1}.")]
        //public string Description { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; }

        public string Hash { get; set; }
        
        public byte Status { get; set; }

        public bool? Unread { get; set; }

        public List<InvoiceItemsViewModel> InvoiceItems { get; set; }
    }
}
