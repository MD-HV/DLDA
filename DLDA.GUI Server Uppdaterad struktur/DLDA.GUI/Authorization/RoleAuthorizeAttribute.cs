using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DLDA.GUI.Authorization
{
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles.Select(r => r.ToLower()).ToArray();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Session.GetString("Role")?.ToLower();

            if (role == null || !_roles.Contains(role))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }
    }
}
