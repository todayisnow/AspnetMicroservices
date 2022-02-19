using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspnetRunBasics
{
    [Authorize(Roles = "admin")]

    public class ContactModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContactModel(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }


        [BindProperty]
        public UserInfoViewModel xm { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            var idpClient = _httpClientFactory.CreateClient("IDPClient");

            var metaDataResponse = await idpClient.GetDiscoveryDocumentAsync();

            if (metaDataResponse.IsError)
            {
                throw new HttpRequestException("Something went wrong while requesting the access token");
            }

            var accessToken = await _httpContextAccessor
                .HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var userInfoResponse = await idpClient.GetUserInfoAsync(
               new UserInfoRequest
               {
                   Address = metaDataResponse.UserInfoEndpoint,
                   Token = accessToken
               });

            if (userInfoResponse.IsError)
            {
                throw new HttpRequestException("Something went wrong while getting user info");
            }

            var userInfoDictionary = new Dictionary<string, string>();

            foreach (var claim in userInfoResponse.Claims)
            {
                if (userInfoDictionary.ContainsKey(claim.Type))
                    userInfoDictionary[claim.Type] = userInfoDictionary[claim.Type] + ", " + claim.Value;
                else
                    userInfoDictionary.Add(claim.Type, claim.Value);
            }
            xm = new UserInfoViewModel(userInfoDictionary);
            return Page();
        }
    }

    public class UserInfoViewModel
    {
        public Dictionary<string, string> userInfoDictionary;

        public UserInfoViewModel(Dictionary<string, string> userInfoDictionary)
        {
            this.userInfoDictionary = userInfoDictionary;
        }
    }
}
