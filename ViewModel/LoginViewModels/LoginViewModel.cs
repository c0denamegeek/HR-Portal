using System.ComponentModel.DataAnnotations;

namespace HR_Portal.ViewModel.LoginViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display (Name = "Remember Me?")]
        public bool RememberMe { get; set; }
    }
}
