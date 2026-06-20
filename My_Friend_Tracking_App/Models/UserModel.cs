

using System.ComponentModel.DataAnnotations;

namespace My_Friend_Tracking_App.Models
{
    // ── Login Model ──────────────────────────────────────────
    public class LoginModel
    {
        [Required(ErrorMessage = "Email zaroor bharo")]
        [EmailAddress(ErrorMessage = "Valid email dalo")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password zaroor bharo")]
        [MinLength(6, ErrorMessage = "Password 6 characters se kam nahi hona chahiye")]
        public string Password { get; set; } = string.Empty;
    }

    // ── Signup Model ─────────────────────────────────────────
    public class SignupModel
    {
        [Required(ErrorMessage = "Full Name zaroor bharo")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email zaroor bharo")]
        [EmailAddress(ErrorMessage = "Valid email dalo")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNo { get; set; }

        [Required(ErrorMessage = "Password zaroor bharo")]
        [MinLength(6, ErrorMessage = "Password kam az kam 6 characters ka ho")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password zaroor bharo")]
        [Compare("Password", ErrorMessage = "Password aur Confirm Password match nahi kar rahe")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // ── User Session / Response Model ────────────────────────
    public class UserSessionModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNo { get; set; }
        public string? ProfileImage { get; set; }
    }

    // ── Location Model ───────────────────────────────────────
    public class UserLocationModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public decimal Latitude { get; set; }
        [Required]
        public decimal Longitude { get; set; }
    }
}