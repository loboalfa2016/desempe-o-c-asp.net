using Microsoft.AspNetCore.Mvc;
using SkillHub.Web.Models;
using System.Text;
using System.Text.Json;

namespace SkillHub.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _httpClientFactory.CreateClient("SkillHubApi");
                var body = JsonSerializer.Serialize(new { model.Email, model.Password });
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/v1/auth/login", content);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Credenciales incorrectas. Verifique su email y contraseña.");
                    return View(model);
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var token = doc.RootElement.GetProperty("token").GetString() ?? string.Empty;
                var fullName = doc.RootElement.GetProperty("fullName").GetString() ?? string.Empty;
                var role = doc.RootElement.GetProperty("role").GetString() ?? string.Empty;

                HttpContext.Session.SetString("JwtToken", token);
                HttpContext.Session.SetString("FullName", fullName);
                HttpContext.Session.SetString("Role", role);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con la API. Asegúrese de que SkillHub.API esté en ejecución.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _httpClientFactory.CreateClient("SkillHubApi");
                var body = JsonSerializer.Serialize(new { model.FullName, model.Email, model.Password });
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/v1/auth/register", content);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Error al registrar. Verifique que el email no esté en uso y que la contraseña tenga al menos 8 caracteres.");
                    return View(model);
                }

                TempData["SuccessMessage"] = "Cuenta creada correctamente. Inicie sesión para continuar.";
                return RedirectToAction("Login");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con la API. Asegúrese de que SkillHub.API esté en ejecución.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
