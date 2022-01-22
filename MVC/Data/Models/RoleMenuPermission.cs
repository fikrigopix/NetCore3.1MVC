using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DFPay.MVC.Data.Models
{
    [Table(name: "AspNetRoleMenuPermission")]
	public class RoleMenuPermission
	{
		[ForeignKey("IdentityRole")]
		public string RoleId { get; set; }
		public IdentityRole IdentityRole { get; set; }

		[ForeignKey("NavigationMenu")]
		public Guid NavigationMenuId { get; set; }

		public NavigationMenu NavigationMenu { get; set; }
    }
}
