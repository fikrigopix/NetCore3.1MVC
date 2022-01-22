using DFPay.Application.ViewModels;
using DFPay.MVC.Data.Context;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace DFPay.MVC.Data.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminUserService(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager
            )
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task AddRoles(string userId, List<string> willAdd)
        {
            var role = _context.Roles.Where(x => willAdd.Contains(x.Id)).ToList();
            var user = await _userManager.FindByIdAsync(userId);
            await _userManager.AddToRolesAsync(user, role.Select(x=> x.NormalizedName));
        }

        public void DeleteRoles(string userId, List<string> willDelete)
        {
            var userRoles = _context.UserRoles.Where(x => x.UserId == userId && willDelete.Contains(x.RoleId));
            _context.UserRoles.RemoveRange(userRoles);
            _context.SaveChanges();
        }

        public List<AdminUserViewModel> GetAdminUser()
        {
            List<AdminUserViewModel> models = new List<AdminUserViewModel>();

            var db_users = _context.Users;
            var db_userRoles = _context.UserRoles;
            var db_roles = _context.Roles;

            foreach (var user in db_users)
            {
                var userRoles = db_userRoles.Where(x => x.UserId == user.Id).Select(x=> x.RoleId).ToList();

                var roles = db_roles.Where(x => userRoles.Contains(x.Id));
                string roleIds = string.Join(",", roles.Select(x=> x.Id));
                string roleNames = string.Join(", ", roles.Select(x => x.Name));

                AdminUserViewModel model = new AdminUserViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    RoleIds = roleIds,
                    RoleNames = roleNames
                };
                models.Add(model);
            }
            return models;
        }

        public AdminUserViewModel GetAdminUserById(string userId)
        {
            var db_user = _context.Users.Where(x => x.Id == userId).FirstOrDefault();
            var db_userRoles = _context.UserRoles;
            var db_roles = _context.Roles;
            var userRoles = db_userRoles.Where(x => x.UserId == userId).Select(x => x.RoleId).ToList();

            var roles = db_roles.Where(x => userRoles.Contains(x.Id));
            string roleIds = string.Join(",", roles.Select(x => x.Id));
            string roleNames = string.Join(", ", roles.Select(x => x.Name));

            AdminUserViewModel model = new AdminUserViewModel
            {
                UserId = userId,
                Email = db_user.Email,
                RoleIds = roleIds,
                RoleNames = roleNames
            };

            return model;
        }

        public List<RoleViewModel> GetAllRoles()
        {
            var role = _context.Roles
                                .Select(x => new RoleViewModel {
                                    Id = x.Id,
                                    Name = x.Name }
                                ).ToList();
            return role;
        }

        public async Task<IdentityResult> AddNewUser(IdentityUser identityUser, string password)
        {
            var result = new IdentityResult();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    result = await _userManager.CreateAsync(identityUser, password);
                    if (result.Succeeded)
                    {
                        //Set email comfirmed
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
                        result = await _userManager.ConfirmEmailAsync(identityUser, code);
                    }
                }
                catch (System.Exception)
                {
                    result = new IdentityResult(); //set result.Succeeded = false
                }
                finally
                {
                    if (!result.Succeeded)
                    {
                        scope.Dispose();
                    }
                    else
                    {
                        scope.Complete();
                    }
                }
            }
            return result;
        }
    }
}
