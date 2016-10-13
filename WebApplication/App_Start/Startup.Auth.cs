﻿using System.Configuration;
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
                        var resource_collection = "users";
                        var resource_id = (context.AuthenticationTicket.Identity as ClaimsIdentity).FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

                        var credential = new ClientCredential("16fe6693-55ec-460d-b3ed-33af78f131e4", "WFQ5YfPYkECZLmX8DPZTdKoHcUdw8B9CV9uYKxOEIBI=");

                        AuthenticationContext authContext = new AuthenticationContext(string.Format("https://login.windows.net/{0}/", tenant));
                        AuthenticationResult result = await authContext.AcquireTokenAsync("https://graph.windows.net/", credential);

                        var requestUrl = $"https://graph.windows.net/{tenant}/{resource_collection}/{resource_id}/getMemberGroups?api-version=1.6";
                        var httpClient = new HttpClient();
                        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                        var body = new StringContent("{\"securityEnabledOnly\": false}", System.Text.Encoding.UTF8, "application/json");
                        request.Content = body;
                        var response = await httpClient.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseString = await response.Content.ReadAsStringAsync();
                            var groupsAsJArray = JObject.Parse(responseString)["value"] as JArray;
                            var groups = string.Join(",", groupsAsJArray);
                            (context.AuthenticationTicket.Identity as ClaimsIdentity)
                                .AddClaim(new System.Security.Claims.Claim("groups", groups));
                        }
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