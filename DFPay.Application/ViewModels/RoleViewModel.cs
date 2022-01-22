using System.ComponentModel.DataAnnotations;
namespace DFPay.Application.ViewModels
{
    public class RoleViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Role Name")]
        public string Name { get; set; }
    }
}
