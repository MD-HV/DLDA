using DLDA.GUI.Authorization;
using Microsoft.AspNetCore.Mvc;

// Skapa/redigera användare (framför allt patienter)

namespace DLDA.GUI.Controllers
{
    [RoleAuthorize("admin")]
    public class AdminUserController : Controller
    {
        private readonly HttpClient _httpClient;

        public AdminUserController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DLDA");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
