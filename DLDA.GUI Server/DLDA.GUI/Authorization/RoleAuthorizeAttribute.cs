using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DLDA.GUI.Authorization
{
    // Anpassad attributklass för åtkomstkontroll baserat på användarroll
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles; // Array för att lagra tillåtna roller

        // Konstruktör för att instansiera klassen med tillåtna roller
        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles.Select(r => r.ToLower()).ToArray(); // Konvertera roller till små bokstäver
        }

        // Metod som körs vid autentisering av åtkomst
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Session.GetString("Role")?.ToLower(); // Hämta användarroll från sessionen och konvertera till små bokstäver

            // Kontrollera om användarrollen är null eller inte finns i listan över tillåtna roller
            if (role == null || !_roles.Contains(role))
            {
                // Om användaren inte har rätt roll, vidarebefordra till inloggningsvyn
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }
    }
}
