using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DFPay.MVC.Data.Services;
using DFPay.MVC.Models;
using DFPay.Application.ViewModels;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;

namespace DFPay.MVC.Controllers
{
    [Authorize("Authorization")]
    public class RoleController : NotifyController
    {
        private readonly ILogger<RoleController> _logger;
        private readonly IRoleService _roleService;
        private readonly IStringLocalizer<DFPay.MVC.Lang.Lang> _loc;

        public RoleController(ILogger<RoleController> logger,
           IRoleService roleService,
           IStringLocalizer<DFPay.MVC.Lang.Lang> loc)
        {
            _logger = logger;
            _roleService = roleService;
            _loc = loc;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetRoleUser()
        {
            List<RoleUserViewModel> models = _roleService.GetRoleUser();
            return Json(new { aaData = models });
        }

        [HttpGet]
        public async Task<IActionResult> ManagePermission (string roleId, string roleName)
        {
            var permissions = new List<NavigationMenuViewModel>();
            if (!string.IsNullOrWhiteSpace(roleId))
            {
                permissions = await _roleService.GetPermissionsByRoleIdAsync(roleId);
            }

            IList<JsTreeViewModel> nodes = new List<JsTreeViewModel>();
            foreach (var item in permissions.OrderBy(x => x.DisplayOrder))
            {
                nodes.Add(new JsTreeViewModel
                {
                    id = item.Id.ToString(),
                    parent = item.ParentMenuId == null ? "#" : item.ParentMenuId.ToString(),
                    text = _loc[item.Name].Value,
                    state = new State { selected = item.Permitted },
                    icon = GetIconForJstree(item.Name)
                });
            }
            ViewBag.RoleName = roleName;
            ViewBag.RoleId = roleId;

            //Serialize to JSON string.
            ViewBag.Json = JsonSerializer.Serialize(nodes);
            return View();
        }

        private string GetIconForJstree(string name)
        {
            if (name.Contains("View"))
            {
                return "fas fa-list-alt";
            }

            if (name.Contains("Get"))
            {
                return "fas fa-globe";
            }

            return name switch
            {
                "Invoice Notification Bell" => "far fa-bell",
                "Dashboard" => "fas fa-th",
                "Invoice" => "fas fa-book",
                "Admin User" => "fas fa-address-card",
                "Role" => "fas fa-user-tag",
                "Create" => "fas fa-plus",
                "Delete" => "fas fa-trash-alt",
                "Resend" => "fas fa-paper-plane",
                "Manage Role" => "fas fa-user-cog",
                "Manage Permission" => "fas fa-user-cog",
                "Submit Permission" => "fas fa-user-check",
                _ => null,
            };
        }

        [HttpPost]
        public async Task<IActionResult> EditRolePermission(string RoleId, string selectedIds)
        {
            if (ModelState.IsValid)
            {
                IEnumerable<Guid> Ids = null;
                if (selectedIds != null)
                {
                    Ids = selectedIds.Split(',').Select(Guid.Parse).ToList();
                }
                
                await _roleService.SetPermissionsByRoleIdAsync(RoleId, Ids);

                Notify(new List<NotifyViewModel>
                {
                    new NotifyViewModel()
                    {
                        Title = _loc["Success"].Value,
                        Message = _loc["Role permission changed"].Value,
                        Type = NotificationType.success
                    }
                });
                return RedirectToAction("Index");
            }

            Notify(new List<NotifyViewModel>
            {
                new NotifyViewModel()
                {
                    Title = _loc["Failed"].Value,
                    Message = _loc["Please contact the admin for support"].Value,
                    Type = NotificationType.error
                }
            });
            return RedirectToAction("Index");
        }

        public IActionResult CreateRole()
        {
            return PartialView("_CreateRole", new RoleViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(RoleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleService.CreateRole(viewModel.Name);
                if (result.Succeeded)
                {
                    Notify(new List<NotifyViewModel>
                    {
                        new NotifyViewModel()
                        {
                            Title = _loc["Success"].Value,
                            Message = _loc["Role has been added successfully"].Value,
                            Type = NotificationType.success
                        }
                    });
                    return RedirectToAction("Index");
                }
                else
                {
                    Notify(new List<NotifyViewModel>
                    {
                        new NotifyViewModel()
                        {
                            Title = _loc["Failed"].Value,
                            Message = _loc["Please contact the admin for support"].Value,
                            Type = NotificationType.error
                        }
                    });
                    ModelState.AddModelError("Name", string.Join(",", result.Errors));
                }
            }
            Notify(new List<NotifyViewModel>
            {
                new NotifyViewModel()
                {
                    Title = _loc["Failed"].Value,
                    Message = _loc["Please contact the admin for support"].Value,
                    Type = NotificationType.error
                }
            });
            return RedirectToAction("Index");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var result = await _roleService.DeleteRole(roleId);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = _loc["Delete successful"].Value });
            }
            else
            {
                return Json(new { success = false, message = _loc["Error while deleting"].Value });
            }
        }
    }
}
