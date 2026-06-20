using Microsoft.AspNetCore.Mvc;
using My_Friend_Tracking_App.Models;
using Newtonsoft.Json;
using System.Text;

namespace Find_my_firend_Frontend.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBase;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            // Agar appsettings.json mein set nahi hai, toh automatic active backend port (7104) uthayega
            _apiBase = config["ApiSettings:BaseUrl"] ?? "https://localhost:7104/api";
        }

        // ==========================================
        // 1. LOGIN MANAGEMENT
        // ==========================================

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
                return RedirectToAction("Index", "Home");

            return View(new LoginModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _httpClientFactory.CreateClient();
                var payload = JsonConvert.SerializeObject(new { email = model.Email, password = model.Password });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiBase}/Users/Login", content);
                var json = await response.Content.ReadAsStringAsync();

                // Generic ApiResponseModel ka use kar ke strongly-typed data read karna
                var result = JsonConvert.DeserializeObject<ApiResponseModel<LoginResponseData>>(json);

                if (response.IsSuccessStatusCode && result?.Data != null)
                {
                    // Session values storage initialization
                    HttpContext.Session.SetInt32("UserId", result.Data.UserId);
                    HttpContext.Session.SetString("FullName", result.Data.FullName);
                    HttpContext.Session.SetString("Email", result.Data.Email);
                    if (!string.IsNullOrEmpty(result.Data.ProfileImage))
                    {
                        HttpContext.Session.SetString("ProfileImage", result.Data.ProfileImage);
                    }

                    TempData["Success"] = "Login successful! Khush Amdeed 👋";
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, result?.Message ?? "Invalid email ya password.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Server connection fault: " + ex.Message);
                return View(model);
            }
        }

        // ==========================================
        // 2. SIGNUP MANAGEMENT
        // ==========================================

        // GET: /Account/Signup
        [HttpGet]
        public IActionResult Signup()
        {
            return View(new SignupModel());
        }

        // POST: /Account/Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(SignupModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _httpClientFactory.CreateClient();

                // Form data mapping for API execution
                var payload = JsonConvert.SerializeObject(new
                {
                    fullName = model.FullName,
                    email = model.Email,
                    password = model.Password,
                    phoneNo = model.PhoneNo
                });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiBase}/Users/Signup", content);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponseModel<object>>(json);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Account ban gaya! Ab login karo.";
                    return RedirectToAction("Login", "Account");
                }

                ModelState.AddModelError(string.Empty, result?.Message ?? "Signup failed. Email already exists?");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "API connection failure error: " + ex.Message);
                return View(model);
            }
        }

        // ==========================================
        // 3. PROFILE MANAGEMENT (INTEGRATED WITH SIZE CHECK & TIMEOUT)
        // ==========================================

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient();
                // SQL Database se fresh user row fetch karne ka endpoint
                var response = await client.GetAsync($"{_apiBase}/Users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponseModel<UserSessionModel>>(json);

                    if (result?.Data != null)
                    {
                        return View(result.Data);
                    }
                }

                TempData["Error"] = "Profile data load karne mein masla aya.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Connection Error: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserSessionModel model, IFormFile? ProfileImageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            model.UserId = userId.Value;

            // Image process karo
            if (ProfileImageFile != null && ProfileImageFile.Length > 0)
            {
                // Size check — 2MB se bada nahi hona chahiye
                if (ProfileImageFile.Length > 2 * 1024 * 1024)
                {
                    TempData["Error"] = "Image 2MB se badi hai. Choti image choose karo.";
                    return View(model);
                }

                using var ms = new MemoryStream();
                await ProfileImageFile.CopyToAsync(ms);
                var bytes = ms.ToArray();
                model.ProfileImage = "data:" + ProfileImageFile.ContentType + ";base64," + Convert.ToBase64String(bytes);
            }
            else
            {
                model.ProfileImage = HttpContext.Session.GetString("ProfileImage");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                // Timeout set karo — Base64 string badi hoti hai
                client.Timeout = TimeSpan.FromSeconds(60);

                var payload = JsonConvert.SerializeObject(new
                {
                    userId = model.UserId,
                    fullName = model.FullName,
                    email = model.Email,
                    phoneNo = model.PhoneNo,
                    profileImage = model.ProfileImage
                });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{_apiBase}/Users/UpdateProfile", content);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponseModel<object>>(json);

                if (response.IsSuccessStatusCode)
                {
                    HttpContext.Session.SetString("FullName", model.FullName);
                    HttpContext.Session.SetString("Email", model.Email);
                    if (!string.IsNullOrEmpty(model.ProfileImage))
                        HttpContext.Session.SetString("ProfileImage", model.ProfileImage);

                    TempData["Success"] = "Profile save ho gayi! 🎉";
                    return RedirectToAction("Profile");
                }

                TempData["Error"] = result?.Message ?? "Profile update nahi hui.";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return View(model);
            }
        }

        // ==========================================
        // 4. LOGOUT MANAGEMENT
        // ==========================================

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Logout successfully completed!";
            return RedirectToAction("Login");
        }
    }
}