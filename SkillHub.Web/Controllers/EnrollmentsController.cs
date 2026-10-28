using Microsoft.AspNetCore.Mvc;
using SkillHub.Web.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SkillHub.Web.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EnrollmentsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("SkillHubApi");
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account", new { returnUrl = "/Enrollments" });

            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync("api/v1/enrollments/mine");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "No se pudieron cargar sus inscripciones.";
                    return View(new List<EnrollmentViewModel>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var enrollments = JsonSerializer.Deserialize<List<EnrollmentViewModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<EnrollmentViewModel>();
                return View(enrollments);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "No se pudo conectar con la API. Asegúrese de que SkillHub.API esté en ejecución.";
                return View(new List<EnrollmentViewModel>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.DeleteAsync($"api/v1/enrollments/{id}");

                if (response.IsSuccessStatusCode)
                    TempData["SuccessMessage"] = "Inscripción cancelada correctamente.";
                else
                    TempData["ErrorMessage"] = "No se pudo cancelar la inscripción.";
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "No se pudo conectar con la API.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int workshopId)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Workshops/Details/{workshopId}" });

            try
            {
                var client = CreateAuthenticatedClient();
                var body = JsonSerializer.Serialize(new { WorkshopId = workshopId });
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/v1/enrollments", content);

                if (response.IsSuccessStatusCode)
                    TempData["SuccessMessage"] = "Inscripción realizada correctamente.";
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    TempData["ErrorMessage"] = "Ya está inscrito en este taller.";
                else
                    TempData["ErrorMessage"] = "No se pudo completar la inscripción. Intente de nuevo.";
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "No se pudo conectar con la API.";
            }

            return RedirectToAction("Details", "Workshops", new { id = workshopId });
        }
    }
}
