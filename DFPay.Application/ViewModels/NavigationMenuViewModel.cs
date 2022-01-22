using System;

namespace DFPay.Application.ViewModels
{
	public class NavigationMenuViewModel
	{
		public string RoleId { get; set; }

		public Guid Id { get; set; }

		public string Name { get; set; }

		public Guid? ParentMenuId { get; set; }

		public string ControllerName { get; set; }

		public string ActionName { get; set; }

        public bool Permitted { get; set; }

        public int DisplayOrder { get; set; }
	}
}
