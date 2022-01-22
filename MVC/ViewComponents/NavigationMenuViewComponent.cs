using DFPay.MVC.Data.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFPay.MVC.ViewComponents
{
	public class NavigationMenuViewComponent : ViewComponent
	{
		private readonly IRoleService _roleService;

		public NavigationMenuViewComponent(IRoleService roleService)
		{
			_roleService = roleService;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var items = await _roleService.GetMenuItemsAsync(HttpContext.User);

			return View(items);
		}
	}
}
