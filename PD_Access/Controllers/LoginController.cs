using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace PD_Access.Controllers
{
    [Authorize]
    public class LoginController : Controller
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginController(ITokenAcquisition tokenAcquisition, IHttpClientFactory httpClientFactory)
        {
            _tokenAcquisition = tokenAcquisition;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            // Always force login if not authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge(new AuthenticationProperties
                {
                    RedirectUri = Url.Action("Index", "Login")
                });
            }

            // Get access token for Graph with required scopes
            var accessToken = "";
            try
            {
                accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { "GroupMember.Read.All" });
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                return Challenge(new AuthenticationProperties
                {
                    RedirectUri = Url.Action("Index", "Login")
                });
            }


            //Get the UserName
            var userName = (User as ClaimsPrincipal)?.FindFirst("name")?.Value;
            HttpContext.Session.SetString("UserDisplayName", userName);

            // Call Graph API to get group memberships
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("me/memberOf");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            // Optionally deserialize to pass strongly typed model to View
            var groups = JsonDocument.Parse(json).RootElement;

            //You can also extract just display names if desired


            var groupsJson = JsonDocument.Parse(json).RootElement;
                       
            if (groupsJson.TryGetProperty("value", out var groupsArray))
            {
                var groupNames = new List<string>();


                foreach (var group in groupsArray.EnumerateArray())
                {
                    if (group.TryGetProperty("displayName", out var displayNameElement))
                    {
                        var displayName = displayNameElement.GetString();

                        if (!string.IsNullOrEmpty(displayName))
                        {
                            var words = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                            if ((words.Length == 2 && words[1].Equals("County", StringComparison.OrdinalIgnoreCase)) ||
                            displayName.Equals("PublicPolicyTest", StringComparison.OrdinalIgnoreCase))
                            {
                                groupNames.Add(displayName);
                            }
                        }
                    }
                }
                if (groupNames.Any(name => name.Equals("PublicPolicyTest", StringComparison.OrdinalIgnoreCase)))
                {
                    HttpContext.Session.SetString("PublicPolicy", "true");
                    HttpContext.Session.SetString("GroupNames", "PublicPolicy");
                }
                else
                {
                    if (groupNames.Count > 0)
                    {
                        ViewBag.GroupsJson = groupNames;
                        var groupNamesJson = JsonSerializer.Serialize(groupNames);
                        HttpContext.Session.SetString("GroupNames", groupNamesJson);
                        ViewBag.hasPublicPolicy = false;
                    }
                    else  // Read Only
                    {
                        HttpContext.Session.SetString("ReadOnly","true" );
                        HttpContext.Session.SetString("GroupNames", "ReadOnly");
                    }

                   
                }
                    

            }



            return Redirect("ModifyPolicy/Index");
            //return View();
        }

        public IActionResult Login()
        {
            var redirectUrl = Url.Action("Index", "Login");
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignOutApp()
        {
            return SignOut(
                new AuthenticationProperties { RedirectUri = "/" },
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public IActionResult Privacy() => View();
    }
}
