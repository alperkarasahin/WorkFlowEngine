using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WorkFlowManager.Common.ViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string Action { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} en azından {2} karakter içermeli.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre(Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifre ve tekrarı aynı olmalı.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "T.C. Kimlik Numarası")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [Display(Name = "Beni Hatırla ?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class RegisterOfInvitationViewModel
    {
        public Guid ConfirmationCode { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Şifre")]
        public string Password { get; set; }
        [Required]
        [DisplayName("Şifre Doğrulama")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string PasswordConfirmation { get; set; }

        [DisplayName("Telefonu")]
        public string PhoneNumber { get; set; }

        [Required]
        [DisplayName("Güvenlik Metni")]
        public string CaptchaResult { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [Display(Name = "T.C. Numarası")]
        public string Tcno { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Şifre en azından {2} karakter uzunluğunda olmalı.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Doğrulama")]
        [Compare("Password", ErrorMessage = "Şifre, doğrulaması ile uyuşmuyor.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Display(Name = "T.C. Kimlik Numarası")]
        public string Tcno { get; set; }
        [Required]
        [Display(Name = "E-posta")]
        public string Email { get; set; }
    }
}
