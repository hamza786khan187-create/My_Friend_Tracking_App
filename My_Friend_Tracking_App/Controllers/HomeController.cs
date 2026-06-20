
using My_Friend_Tracking_App.Models;

using Microsoft.AspNetCore.Mvc;
using My_Friend_Tracking_App.Models;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace Find_my_firend_Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBase;

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _apiBase = config["ApiSettings:BaseUrl"] ?? "https://localhost:7104/api";
        }

        // GET: /Home/Index (Main Tracker Map Frame)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var vm = new MapViewModel
            {
                CurrentUser = new UserSessionModel
                {
                    UserId = userId.Value,
                    FullName = HttpContext.Session.GetString("FullName") ?? "User",
                    Email = HttpContext.Session.GetString("Email") ?? ""
                }
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{_apiBase}/Friends/MyFriends/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponseModel<List<FriendModel>>>(json);
                    vm.FriendMarkers = result?.Data ?? new List<FriendModel>();
                }
            }
            catch
            {
                vm.FriendMarkers = new List<FriendModel>();
            }

            return View(vm);
        }

        // POST: /Home/SaveLocation (Real-time GPS Coordinate Loop Execution)
        [HttpPost]
        public async Task<IActionResult> SaveLocation([FromBody] UserLocationModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Session expired or identity unverified." });

            model.UserId = userId.Value;

            try
            {
                var client = _httpClientFactory.CreateClient();
                var payload = JsonConvert.SerializeObject(model);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{_apiBase}/Users/SavedUserLocation", content);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponseModel<object>>(json);

                return Json(new { success = response.IsSuccessStatusCode, message = result?.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}