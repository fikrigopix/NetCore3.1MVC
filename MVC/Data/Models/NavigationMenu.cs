using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFPay.MVC.Data.Models
{
    [Table(name: "AspNetNavigationMenu")]
    public class NavigationMenu
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }

		public string Name { get; set; }

		[ForeignKey("ParentNavigationMenu")]
		public Guid? ParentMenuId { get; set; }

		public virtual NavigationMenu ParentNavigationMenu { get; set; }

		public string ControllerName { get; set; }

		public string ActionName { get; set; }

		public int DisplayOrder { get; set; }
	}
}
