using DFPay.Application.ViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DFPay.MVC.Data.Services
{
    public interface IRoleService
    {
        Task<List<NavigationMenuViewModel>> GetPermissionsByRoleIdAsync(string id);
        Task<bool> SetPermissionsByRoleIdAsync(string id, IEnumerable<Guid> permissionIds);
        List<RoleUserViewModel> GetRoleUser();

        Task<bool> GetMenuItemsAsync(ClaimsPrincipal ctx, string ctrl, string act);
        Task<List<NavigationMenuViewModel>> GetMenuItemsAsync(ClaimsPrincipal principal);

        Task<IdentityResult> CreateRole(string roleName);
        Task<IdentityResult> DeleteRole(string roleId);
    }
}