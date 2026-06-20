

namespace My_Friend_Tracking_App.Models
{
    // ── Generic API Response ─────────────────────────────────
    // Yeh model API se jo JSON aata hai usse map karta hai
    public class ApiResponseModel<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    // ── Login API Response Data ──────────────────────────────
    public class LoginResponseData
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNo { get; set; }
        public string? ProfileImage { get; set; }
    }
}