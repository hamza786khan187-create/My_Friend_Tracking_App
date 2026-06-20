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

                // 1. Accepted Friends List
                var responseFriends = await client.GetAsync($"{_apiBase}/Friends/MyFriends/{userId}");
                if (responseFriends.IsSuccessStatusCode)
                {
                    var jsonFriends = await responseFriends.Content.ReadAsStringAsync();
                    var resultFriends = JsonConvert.DeserializeObject<ApiResponseModel<List<FriendModel>>>(jsonFriends);
                    vm.Contacts = resultFriends?.Data ?? new List<FriendModel>();
                }

                // 2. Pending Inbound Requests
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

        // POST: /Friends/SendRequest  (by User ID)
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
                    TempData["Success"] = result?.Message ?? "Friend Request bhej di gayi! 🚀";
                else
                    TempData["Error"] = result?.Message ?? "Request bhejne mein nakami hui.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Network Connection Error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: /Friends/SendRequestByEmail  (by Gmail/Email)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequestByEmail(string email)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Email address daalna zaroori hai.";
                return RedirectToAction("Index");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var payload = JsonConvert.SerializeObject(new { senderId = userId, email = email.Trim() });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiBase}/Friends/SendRequestByEmail", content);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponseModel<object>>(json);

                if (response.IsSuccessStatusCode)
                    TempData["Success"] = result?.Message ?? $"📧 {email} ko invite bhej diya gaya!";
                else
                    TempData["Error"] = result?.Message ?? "Email se request bhejne mein error aya.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Network Error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: /Friends/SendRequestByPhone  (by Phone Number)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequestByPhone(string phoneNo)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(phoneNo))
            {
                TempData["Error"] = "Phone number daalna zaroori hai.";
                return RedirectToAction("Index");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var payload = JsonConvert.SerializeObject(new { senderId = userId, phoneNo = phoneNo.Trim() });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiBase}/Friends/SendRequestByPhone", content);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponseModel<object>>(json);

                if (response.IsSuccessStatusCode)
                    TempData["Success"] = result?.Message ?? $"📱 {phoneNo} ko invite bhej diya gaya!";
                else
                    TempData["Error"] = result?.Message ?? "Phone number se request bhejne mein error aya.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Network Error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: /Friends/RespondRequest  (Accept / Reject)
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
                    TempData["Success"] = status.ToLower() == "accepted"
                        ? "Request Accept ho gayi! 🎉"
                        : "Request Reject kar di gayi.";
                else
                    TempData["Error"] = result?.Message ?? "Action perform karne mein error aya.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Response pipeline error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: /Friends/UpdateLocation  (Live location share karo)
        [HttpPost]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateRequest req)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized(new { message = "Session expire ho gayi hai. Dobara login karein." });

            try
            {
                var client = _httpClientFactory.CreateClient();
                var payload = JsonConvert.SerializeObject(new
                {
                    userId = userId.Value,
                    latitude = req.Latitude,
                    longitude = req.Longitude
                });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiBase}/Location/UpdateLocation", content);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return Content(json, "application/json");

                return StatusCode((int)response.StatusCode, json);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Location update error: " + ex.Message });
            }
        }

        // POST: /Friends/RemoveFriend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFriend(int friendId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.DeleteAsync($"{_apiBase}/Friends/Remove/{friendId}/{userId}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponseModel<object>>(json);

                if (response.IsSuccessStatusCode)
                    TempData["Success"] = result?.Message ?? "Friend remove ho gaya.";
                else
                    TempData["Error"] = result?.Message ?? "Friend remove karne mein error aya.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }

    // ── Helper request model for /Friends/UpdateLocation ────
    public class LocationUpdateRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}














//using Microsoft.AspNetCore.Mvc;
//using My_Friend_Tracking_App.Models;
//using Newtonsoft.Json;
//using System.Text;

//namespace Find_my_firend_Frontend.Controllers
//{
//    public class FriendsController : Controller
//    {
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly string _apiBase;

//        public FriendsController(IHttpClientFactory httpClientFactory, IConfiguration config)
//        {
//            _httpClientFactory = httpClientFactory;
//            // Agar config mein na mile toh direct aapki running backend port (7104) hit hogi
//            _apiBase = config["ApiSettings:BaseUrl"] ?? "https://localhost:7104/api";
//        }

//        // GET: /Friends/Index
//        [HttpGet]
//        public async Task<IActionResult> Index()
//        {
//            var userId = HttpContext.Session.GetInt32("UserId");
//            if (userId == null)
//                return RedirectToAction("Login", "Account");

//            var vm = new FriendsViewModel();

//            try
//            {
//                var client = _httpClientFactory.CreateClient();

//                // 1. Accepted Friends List load karna
//                var responseFriends = await client.GetAsync($"{_apiBase}/Friends/MyFriends/{userId}");
//                if (responseFriends.IsSuccessStatusCode)
//                {
//                    var jsonFriends = await responseFriends.Content.ReadAsStringAsync();
//                    var resultFriends = JsonConvert.DeserializeObject<ApiResponseModel<List<FriendModel>>>(jsonFriends);
//                    vm.Contacts = resultFriends?.Data ?? new List<FriendModel>();
//                }

//                // 2. Pending Inbound Requests load karna
//                var responsePending = await client.GetAsync($"{_apiBase}/Friends/PendingRequests/{userId}");
//                if (responsePending.IsSuccessStatusCode)
//                {
//                    var jsonPending = await responsePending.Content.ReadAsStringAsync();
//                    var resultPending = JsonConvert.DeserializeObject<ApiResponseModel<List<FriendModel>>>(jsonPending);
//                    vm.PendingRequests = resultPending?.Data ?? new List<FriendModel>();
//                }
//            }
//            catch
//            {
//                vm.Contacts = new List<FriendModel>();
//                vm.PendingRequests = new List<FriendModel>();
//            }

//            return View(vm);
//        }

//        // POST: /Friends/SendRequest
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> SendRequest(int receiverId)
//        {
//            var userId = HttpContext.Session.GetInt32("UserId");
//            if (userId == null)
//                return RedirectToAction("Login", "Account");

//            try
//            {
//                var client = _httpClientFactory.CreateClient();
//                var payload = JsonConvert.SerializeObject(new { senderId = userId, receiverId });
//                var content = new StringContent(payload, Encoding.UTF8, "application/json");

//                var response = await client.PostAsync($"{_apiBase}/Friends/SendRequest", content);
//                var json = await response.Content.ReadAsStringAsync();
//                var result = JsonConvert.DeserializeObject<ApiResponseModel<object>>(json);

//                if (response.IsSuccessStatusCode)
//                {
//                    TempData["Success"] = result?.Message ?? "Friend Request bhej di gayi hai! 🚀";
//                }
//                else
//                {
//                    TempData["Error"] = result?.Message ?? "Request bhejne mein nakami hui.";
//                }
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = "Network Connection Error: " + ex.Message;
//            }

//            return RedirectToAction("Index");
//        }

//        // POST: /Friends/RespondRequest
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> RespondRequest(int friendId, string status)
//        {
//            var userId = HttpContext.Session.GetInt32("UserId");
//            if (userId == null)
//                return RedirectToAction("Login", "Account");

//            try
//            {
//                var client = _httpClientFactory.CreateClient();
//                var payload = JsonConvert.SerializeObject(new { friendId, status });
//                var content = new StringContent(payload, Encoding.UTF8, "application/json");

//                var response = await client.PutAsync($"{_apiBase}/Friends/RespondRequest", content);
//                var json = await response.Content.ReadAsStringAsync();
//                var result = JsonConvert.DeserializeObject<ApiResponseModel<object>>(json);

//                if (response.IsSuccessStatusCode)
//                {
//                    TempData["Success"] = status.ToLower() == "accepted" ? "Request Accept ho gayi! 🎉" : "Request Reject kar di gayi.";
//                }
//                else
//                {
//                    TempData["Error"] = result?.Message ?? "Action perform karne mein error aya.";
//                }
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = "Response pipeline error: " + ex.Message;
//            }

//            return RedirectToAction("Index");
//        }
//    }
//}