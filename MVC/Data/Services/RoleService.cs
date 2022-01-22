using DFPay.MVC.Data.Context;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using DFPay.MVC.Data.Models;
using System.Security.Claims;
using DFPay.Application.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace DFPay.MVC.Data.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleService(ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public List<RoleUserViewModel> GetRoleUser()
        {
            List<RoleUserViewModel> models = new List<RoleUserViewModel>();

            var db_users = _context.Users;
            var db_userRoles = _context.UserRoles;
            var db_roles = _context.Roles;

            foreach (var role in db_roles)
            {
                var userRoles = db_userRoles.Where(x => x.RoleId == role.Id).Select(x => x.UserId).ToList();

                var users = db_users.Where(x => userRoles.Contains(x.Id));
                string userIds = string.Join(",", users.Select(x => x.Id));
                string emails = string.Join(", ", users.Select(x => x.Email));

                RoleUserViewModel model = new RoleUserViewModel
                {
                     RoleId = role.Id,
                     RoleName = role.Name,
                     UserIds = userIds,
                     Emails = emails
                };
                models.Add(model);
            }
            return models;
        }

        public async Task<List<NavigationMenuViewModel>> GetPermissionsByRoleIdAsync(string id)
        {
            var items = await (from m in _context.NavigationMenu
                               join rm in _context.RoleMenuPermission
                                on new { X1 = m.Id, X2 = id } equals new { X1 = rm.NavigationMenuId, X2 = rm.RoleId }
                                into rmp
                               from rm in rmp.DefaultIfEmpty()
                               select new NavigationMenuViewModel()
                               {
                                   RoleId = id,
                                   Id = m.Id,
                                   Name = m.Name,
                                   ActionName = m.ActionName,
                                   ControllerName = m.ControllerName,
                                   DisplayOrder = m.DisplayOrder,
                                   ParentMenuId = m.ParentMenuId,
                                   Permitted = rm.RoleId == id
                               })
                               .AsNoTracking()
                               .ToListAsync();

            return items;
        }

        //Remove old permissions for that role id and assign changed permissions
        public async Task<bool> SetPermissionsByRoleIdAsync(string id, IEnumerable<Guid> permissionIds)
        {
            var existing = await _context.RoleMenuPermission.Where(x => x.RoleId == id).ToListAsync();
            _context.RemoveRange(existing);

            if (permissionIds != null)
            {
                foreach (var item in permissionIds)
                {
                    await _context.RoleMenuPermission.AddAsync(new RoleMenuPermission()
                    {
                        RoleId = id,
                        NavigationMenuId = item,
                    });
                }
            }

            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> GetMenuItemsAsync(ClaimsPrincipal ctx, string ctrl, string act)
        {
            var result = false;
            var roleIds = await GetUserRoleIds(ctx);
            var data = await (from menu in _context.RoleMenuPermission
                              where roleIds.Contains(menu.RoleId)
                              select menu)
                              .Select(m => m.NavigationMenu).Distinct().ToListAsync();

            foreach (var item in data)
            {
                result = (item.ControllerName == ctrl && item.ActionName == act);
                if (result)
                    break;
            }

            return result;
        }

        private async Task<List<string>> GetUserRoleIds(ClaimsPrincipal ctx)
        {
            var userId = GetUserId(ctx);
            var data = await (from role in _context.UserRoles
                              where role.UserId == userId
                              select role.RoleId).ToListAsync();

            return data;
        }

        private string GetUserId(ClaimsPrincipal user)
        {
            return ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<List<NavigationMenuViewModel>> GetMenuItemsAsync(ClaimsPrincipal principal)
        {
            var isAuthenticated = principal.Identity.IsAuthenticated;
            if (!isAuthenticated)
                return new List<NavigationMenuViewModel>();

            var roleIds = await GetUserRoleIds(principal);
            var data = await (from menu in _context.RoleMenuPermission
                              where roleIds.Contains(menu.RoleId)
                              select menu)
                              .Select(m => new NavigationMenuViewModel()
                              {
                                  Id = m.NavigationMenu.Id,
                                  Name = m.NavigationMenu.Name,
                                  ActionName = m.NavigationMenu.ActionName,
                                  ControllerName = m.NavigationMenu.ControllerName,
                                  DisplayOrder = m.NavigationMenu.DisplayOrder,
                                  ParentMenuId = m.NavigationMenu.ParentMenuId,
                                  Permitted = true
                              }).Distinct().ToListAsync();

            return data;
        }

        public async Task<IdentityResult> CreateRole(string roleName)
        {
            return await _roleManager.CreateAsync(new IdentityRole() { Name = roleName });
        }

        public async Task<IdentityResult> DeleteRole(string roleId)
        {
            var identityRole = await _roleManager.FindByIdAsync(roleId);
            return await _roleManager.DeleteAsync(identityRole);
        }
    }
}
