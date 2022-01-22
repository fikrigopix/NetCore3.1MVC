using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DFPay.MVC.Data.Services;
using System.Collections.Generic;
using System.Linq;
using DFPay.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace DFPay.MVC.Controllers
{
    [Authorize("Authorization")]
    public class AdminUserController : NotifyController
    {
        private readonly ILogger<AdminUserController> _logger;
        private IAdminUserService _adminUserService;
        private readonly IStringLocalizer<DFPay.MVC.Lang.Lang> _loc;

        public AdminUserController(
            ILogger<AdminUserController> logger,
            IAdminUserService adminUserService,
            IStringLocalizer<DFPay.MVC.Lang.Lang> loc)
        {
            _logger = logger;
            _adminUserService = adminUserService;
            _loc = loc;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAdminUser()
        {
            List<AdminUserViewModel> models = _adminUserService.GetAdminUser();
            return Json(new { aaData = models });
        }

        [HttpGet]
        public IActionResult ManageRole(string userId)
        {
            var adminUser = _adminUserService.GetAdminUserById(userId);

            EditRoleViewModel editRole = new EditRoleViewModel
            {
                UserId = adminUser.UserId,
                Email = adminUser.Email,
                Roles = _adminUserService.GetAllRoles(),
                SelectedRoleArray = adminUser.RoleIds.Split(',').ToArray()
            };

            return PartialView("_ManageRole", editRole);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRole(EditRoleViewModel model)
        {
            if (!ModelState.IsValid)
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
                return RedirectToAction("Index");
            }
            var user = _adminUserService.GetAdminUserById(model.UserId);
            if (user == null)
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
                return RedirectToAction("Index");
            }

            var oldRole = user.RoleIds.Split(',').ToArray();

            var willDelete = oldRole.Where(x => !model.SelectedRoleArray.Contains(x)).ToList();
            if (willDelete.Count > 0 && willDelete.FirstOrDefault() != string.Empty)
            {
                _adminUserService.DeleteRoles(user.UserId, willDelete);
            }

            var willAdd = model.SelectedRoleArray.Where(x => !oldRole.Contains(x)).ToList();
            if (willAdd.Count > 0 && willAdd.FirstOrDefault() != string.Empty)
            {
                await _adminUserService.AddRoles(user.UserId, willAdd);
            }

            Notify(new List<NotifyViewModel>
            {
                new NotifyViewModel()
                {
                    Title = _loc["Success"].Value,
                    Message = _loc["User roles updated successfully"].Value,
                    Type = NotificationType.success
                }
            });
            return RedirectToAction("Index");
        }

        public IActionResult CreateUser()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Notify(new List<NotifyViewModel>
                {
                    new NotifyViewModel()
                    {
                        Title = _loc["Failed"].Value,
                        Message = _loc["Failed while create user"].Value,
                        Type = NotificationType.success
                    }
                });
                return RedirectToAction("Index");
            }

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _adminUserService.AddNewUser(user, model.Password);
            if (!result.Succeeded)
            {
                var notifies = new List<NotifyViewModel>();
                foreach (var item in result.Errors)
                {
                    notifies.Add(new NotifyViewModel()
                    {
                        Title = _loc["Failed"].Value,
                        Message = _loc[item.Description].Value,
                        Type = NotificationType.error
                    });
                }
                Notify(notifies);
                return View(model);
            }

            Notify(new List<NotifyViewModel>
            {
                new NotifyViewModel()
                {
                    Title = _loc["Success"].Value,
                    Message = _loc["User created successfully"].Value,
                    Type = NotificationType.success
                }
            });
            return RedirectToAction("Index");
        }
    }
}
