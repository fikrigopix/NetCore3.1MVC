using DFPay.Application.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFPay.MVC.Data.Services
{
    public interface IAdminUserService
    {
        List<AdminUserViewModel> GetAdminUser();
        AdminUserViewModel GetAdminUserById(string userId);
        List<RoleViewModel> GetAllRoles();
        void DeleteRoles(string userId, List<string> willDelete);
        Task AddRoles(string userId, List<string> willAdd);
        Task<IdentityResult> AddNewUser(IdentityUser identityUser, string password);
    }
}
