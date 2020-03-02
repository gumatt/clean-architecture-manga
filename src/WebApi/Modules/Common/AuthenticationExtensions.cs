namespace WebApi.Modules.Common
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Json;
    using Domain.Security.Services;
    using Infrastructure.ExternalAuthentication;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    // https://www.jerriepelser.com/blog/authenticate-oauth-aspnet-core-2/
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            bool useFake = configuration.GetValue<bool>("AuthenticationModule:UseFake");
            if (useFake)
            {
                services.AddSingleton<IUserService, TestUserService>();
            }
            else
            {
                services.AddScoped<IUserService, GitHubUserService>();
                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    })
                    .AddCookie()
                    .AddOAuth("Google", options =>
                    {
                        options.ClientId = configuration["AuthenticationModule:Google:ClientId"];
                        options.ClientSecret = configuration["AuthenticationModule:Google:ClientSecret"];
                        options.CallbackPath = new PathString("/api/v1/Google/LoginCallback");

                        options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
                        options.TokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
                        options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                        options.Scope.Add("openid");
                        options.Scope.Add("profile");
                        options.Scope.Add("email");

                        options.SaveTokens = true;

                        options.ClaimActions.Clear();
                        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                        options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                        options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                        options.ClaimActions.MapJsonKey("urn:google:profile", "link");
                        options.ClaimActions.MapJsonKey("image", "picture");
                        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

                        options.Events = new OAuthEvents
                        {
                            OnCreatingTicket = async context =>
                            {
                                var request = new HttpRequestMessage(
                                    HttpMethod.Get,
                                    context.Options.UserInformationEndpoint);
                                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                request.Headers.Authorization =
                                    new AuthenticationHeaderValue("Bearer", context.AccessToken);

                                var response = await context
                                    .Backchannel
                                    .SendAsync(
                                        request,
                                        HttpCompletionOption.ResponseHeadersRead,
                                        context.HttpContext
                                            .RequestAborted)
                                    .ConfigureAwait(false);
                                response.EnsureSuccessStatusCode();

                                var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()
                                    .ConfigureAwait(false));

                                context.RunClaimActions(user.RootElement);
                            }
                        };
                    })
                    .AddOAuth("GitHub", options =>
                    {
                        options.ClientId = configuration["AuthenticationModule:GitHub:ClientId"];
                        options.ClientSecret = configuration["AuthenticationModule:GitHub:ClientSecret"];
                        options.CallbackPath = new PathString("/api/v1/GitHub/LoginCallback");

                        options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                        options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                        options.UserInformationEndpoint = "https://api.github.com/user";

                        options.SaveTokens = true;

                        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                        options.ClaimActions.MapJsonKey("urn:github:login", "login");
                        options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
                        options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");

                        options.Events = new OAuthEvents
                        {
                            OnCreatingTicket = async context =>
                            {
                                var request = new HttpRequestMessage(
                                    HttpMethod.Get,
                                    context.Options.UserInformationEndpoint);
                                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                request.Headers.Authorization =
                                    new AuthenticationHeaderValue("Bearer", context.AccessToken);

                                var response = await context
                                    .Backchannel
                                    .SendAsync(
                                        request,
                                        HttpCompletionOption.ResponseHeadersRead,
                                        context.HttpContext.RequestAborted)
                                    .ConfigureAwait(false);
                                response.EnsureSuccessStatusCode();

                                var user = JsonDocument.Parse(await response
                                    .Content.
                                    ReadAsStringAsync()
                                    .ConfigureAwait(false));

                                context.RunClaimActions(user.RootElement);
                            }
                        };
                    });
            }


            return services;
        }
    }
}
