using Microsoft.AspNetCore.Mvc;
using SkillHub.Web.Models;
using System.Text.Json;

namespace SkillHub.Web.Controllers
{
    public class WorkshopsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WorkshopsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SkillHubApi");
                var response = await client.GetAsync("api/v1/workshops");
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = $"No se pudieron cargar los talleres (código {(int)response.StatusCode}).";
                    return View(new List<WorkshopViewModel>());
                }
                var json = await response.Content.ReadAsStringAsync();
                var workshops = JsonSerializer.Deserialize<List<WorkshopViewModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<WorkshopViewModel>();
                return View(workshops);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "No se pudo conectar con la API. Asegúrese de que SkillHub.API esté en ejecución.";
                return View(new List<WorkshopViewModel>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SkillHubApi");
                var response = await client.GetAsync($"api/v1/workshops/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return NotFound();
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = $"No se pudo cargar el taller (código {(int)response.StatusCode}).";
                    return View((WorkshopViewModel?)null);
                }
                var json = await response.Content.ReadAsStringAsync();
                var workshop = JsonSerializer.Deserialize<WorkshopViewModel>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(workshop);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "No se pudo conectar con la API. Asegúrese de que SkillHub.API esté en ejecución.";
                return View((WorkshopViewModel?)null);
            }
        }
    }
}
