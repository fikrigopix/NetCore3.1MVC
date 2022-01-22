using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DFPay.Application.ViewModels
{
    public class EditRoleViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }

        public IEnumerable<RoleViewModel> Roles { get; set; }

        [Required]
        public string[] SelectedRoleArray { get; set; }
    }
}
