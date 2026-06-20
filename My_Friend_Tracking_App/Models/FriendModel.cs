

using My_Friend_Tracking_App.Models;
using System.ComponentModel.DataAnnotations;

namespace My_Friend_Tracking_App.Models
{
    // ── Friend / Contact Card ────────────────────────────────
    public class FriendModel
    {
        public int FriendId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNo { get; set; }
        public string? ProfileImage { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Status { get; set; } = "Pending";  // Pending / Accepted / Rejected
        public bool IsOnline { get; set; }
    }

    // ── Send Friend Request Model ────────────────────────────
    public class SendRequestModel
    {
        [Required]
        public int SenderId { get; set; }
        [Required]
        public int ReceiverId { get; set; }
    }

    // ── Respond to Request Model ─────────────────────────────
    public class RespondRequestModel
    {
        [Required]
        public int FriendId { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty;  // Accepted / Rejected
    }

    // ── Friends Page ViewModel ───────────────────────────────
    public class FriendsViewModel
    {
        public List<FriendModel> Contacts { get; set; } = new();
        public List<FriendModel> PendingRequests { get; set; } = new();
        public List<int> SentRequestIds { get; set; } = new();
    }

    // ── Map Page ViewModel ───────────────────────────────────
    public class MapViewModel
    {
        public UserSessionModel? CurrentUser { get; set; }
        public List<FriendModel> FriendMarkers { get; set; } = new();
    }
}