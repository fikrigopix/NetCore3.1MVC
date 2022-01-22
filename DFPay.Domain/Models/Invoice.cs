using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DFPay.Domain.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(15)]
        public string InvoiceNo { get; set; }

        [Required]
        [StringLength(50)]
        public string ClientName { get; set; }

        [Required]
        [StringLength(200)]
        public string ClientEmail { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        public DateTime ValidDate { get; set; }

        [Required]
        [StringLength(70)]
        public string Hash { get; set; }

        public DateTime? PaymentDate { get; set; }

        [Required]
        public byte Status { get;set; }

        public bool? Unread { get; set; }

        public List<InvoiceItem> InvoiceItems { get; set; }
    }
}
