using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFPay.Application.ViewModels
{
    public class InvoiceItemsViewModel
    {
        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Description")]
        [StringLength(200, ErrorMessage = "{0} length can not be more than {1}.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Unit Price")]
        [Range(0.01, 999999999999999.99, ErrorMessage = "{0} must be greater than {1} and less than {2}")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Total Price")]
        [Range(0.01, 999999999999999.99, ErrorMessage = "{0} must be greater than {1} and less than {2}")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }
    }
}
