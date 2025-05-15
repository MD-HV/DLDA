using DLDA.GUI.Authorization;
using Microsoft.AspNetCore.Mvc;

// Hantera frågedefinitioner: skapa, redigera, ta bort, lista

namespace DLDA.GUI.Controllers
{
    [RoleAuthorize("admin")]
    public class AdminQuestionController : Controller
    {
        private readonly HttpClient _httpClient;

        public AdminQuestionController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DLDA");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
