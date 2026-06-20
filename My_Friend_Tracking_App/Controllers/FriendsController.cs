using Microsoft.AspNetCore.Mvc;
using My_Friend_Tracking_App.Models;
using Newtonsoft.Json;
using System.Text;

namespace Find_my_firend_Frontend.Controllers
{
    public class FriendsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBase;

        public FriendsController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            // Agar config mein na mile toh direct aapki running backend port (7104) hit hogi
            _apiBase = config["ApiSettings:BaseUrl"] ?? "https://localhost:7104/api";
        }

        // GET: /Friends/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var vm = new FriendsViewModel();

            try
            {
                var client = _httpClientFactory.CreateClient();

                // 1. Accepted Friends List load karna
                var responseFriends = await client.GetAsync($"{_apiBase}/Friends/MyFriends/{userId}");
                if (responseFriends.IsSuccessStatusCode)
                {
                    var jsonFriends = await responseFriends.Content.ReadAsStringAsync();
                    var resultFriends = JsonConvert.DeserializeObject<ApiResponseModel<List<FriendModel>>>(jsonFriends);
                    vm.Contacts = resultFriends?.Data ?? new List<FriendModel>();
                }

                // 2. Pending Inbound Requests load karna
                var responsePending = await client.GetAsync($"{_apiBase}/Friends/PendingRequests/{userId}");
                if (responsePending.IsSuccessStatusCode)
                {
                    var jsonPending = await responsePending.Content.ReadAsStringAsync();
                    var resultPending = JsonConvert.DeserializeObject<ApiResponseModel<List<FriendModel>>>(jsonPending);
                    vm.PendingRequests = resultPending?.Data ?? new List<FriendModel>();
                }
            }
            catch
            {
                vm.Contacts = new List<FriendModel>();
                vm.PendingRequests = new List<FriendModel>();
            }

            return View(vm);
        }

        // POST: /Friends/SendRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequest(int receiverId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient();
                var payload = JsonConvert.SerializeObject(new { senderId = userId, receiverId });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiBase}/Friends/SendRequest", content);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponseModel<object>>(json);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = result?.Message ?? "Friend Request bhej di gayi hai! 🚀";
                }
                else
                {
                    TempData["Error"] = result?.Message ?? "Request bhejne mein nakami hui.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Network Connection Error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: /Friends/RespondRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RespondRequest(int friendId, string status)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient();
                var payload = JsonConvert.SerializeObject(new { friendId, status });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{_apiBase}/Friends/RespondRequest", content);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponseModel<object>>(json);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = status.ToLower() == "accepted" ? "Request Accept ho gayi! 🎉" : "Request Reject kar di gayi.";
                }
                else
                {
                    TempData["Error"] = result?.Message ?? "Action perform karne mein error aya.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Response pipeline error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}