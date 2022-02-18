using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace AspnetRunBasics.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly IHttpContextAccessor _accessor;

        public LogoutModel(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public async Task OnGetAsync()
        {
            await _accessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _accessor.HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

        }
    }
}
