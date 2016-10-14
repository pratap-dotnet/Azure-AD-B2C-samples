using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json.Linq;
using Owin;

namespace WebApplication
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AadInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        public static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];

        // B2C policy identifiers
        public static string SignUpPolicyId = ConfigurationManager.AppSettings["ida:SignUpPolicyId"];
        public static string SignInPolicyId = ConfigurationManager.AppSettings["ida:SignInPolicyId"];
        public static string ProfilePolicyId = ConfigurationManager.AppSettings["ida:UserProfilePolicyId"];

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(SignInPolicyId));
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(SignUpPolicyId));
            app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(ProfilePolicyId));
        }

        private OpenIdConnectAuthenticationOptions CreateOptionsFromPolicy(string policy)
        {
            return new OpenIdConnectAuthenticationOptions
            {
                MetadataAddress = string.Format(aadInstance, tenant, policy),
                AuthenticationType = policy,
                ClientId = clientId,
                RedirectUri = redirectUri,
                PostLogoutRedirectUri = redirectUri,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = OnAuthenticationFailed,
                    AuthorizationCodeReceived = async (context) =>
                    {
                        var resource_id = (context.AuthenticationTicket.Identity as ClaimsIdentity).FindFirst(ClaimTypes.NameIdentifier).Value;

                        var credential = new ClientCredential("16fe6693-55ec-460d-b3ed-33af78f131e4", "WFQ5YfPYkECZLmX8DPZTdKoHcUdw8B9CV9uYKxOEIBI=");

                        AuthenticationContext authContext = new AuthenticationContext(string.Format("https://login.windows.net/{0}/", tenant));
                        AuthenticationResult result = await authContext.AcquireTokenAsync("https://graph.windows.net/", credential);
                        var userGroups = await GetUserGroups(resource_id, result.AccessToken);
                        var groupNames = await GetGroups(result.AccessToken);

                        List<string> guNames = new List<string>();
                        foreach (var userGroup in userGroups)
                        {
                            string gName = string.Empty;
                            if (groupNames.TryGetValue(userGroup, out gName))
                            {
                                guNames.Add(gName);
                            }
                        }

                        var groups = string.Join(",", guNames);
                        (context.AuthenticationTicket.Identity as ClaimsIdentity)
                            .AddClaim(new Claim("groups", groups));
                        return;

                    }
                },
                //Scope = "openid",
                //ResponseType = "id_token",
                TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                {
                    NameClaimType = "name",
                    SaveSigninToken = true
                }
            };
        }

        private async Task<IDictionary<string, string>> GetGroups(string accessToken)
        {
            var requestUrl = $"https://graph.windows.net/{tenant}/groups?api-version=1.6";
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.SendAsync(request);
            var result = new Dictionary<string, string>();
            if (response.IsSuccessStatusCode)
            {
                var responseAsString = await response.Content.ReadAsStringAsync();
                var groupAsJArry = JObject.Parse(responseAsString)["value"] as JArray;

                foreach (var group in groupAsJArry)
                {
                    result.Add(group["objectId"].ToString(), group["displayName"].ToString());
                }
            }
            return result;
        }

        private static async Task<IEnumerable<string>> GetUserGroups(string userId, string accessToken)
        {
            var requestUrl = $"https://graph.windows.net/{tenant}/users/{userId}/getMemberGroups?api-version=1.6";
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var body = new StringContent("{\"securityEnabledOnly\": false}", System.Text.Encoding.UTF8, "application/json");
            request.Content = body;
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var groupsAsJArray = JObject.Parse(responseString)["value"] as JArray;

                return groupsAsJArray.Select(a => a.ToString());
            }
            return new List<string>();
        }

        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            if (notification.Exception.Message == "access_denied")
            {
                notification.Response.Redirect("/");
            }
            else
            {
                notification.Response.Redirect("/Home/Error?message=" + notification.Exception.Message);
            }

            return Task.FromResult(0);
        }
    }
}