using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using react_playground.Controllers;
using System.Net;
using System.Security.Claims;

namespace react_playground
{
    public class TransformWinAuthToJWT
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyServerClaimsMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        public TransformWinAuthToJWT(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context/*, UserFacade userFacade*/)
        {
            string requestPath = context.Request.Path.ToString().ToLower(); // normalizace PATH
            bool signIn = requestPath.StartsWith("/account/signin");

            if (context.User.Identity?.IsAuthenticated ?? false) // na produkci by mel byt autentizovany vzdy, to zajisti WIN AUTH na IIS
            {
                if (context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value != null)
                {
                    if (!signIn && !context.User.Claims.Any(x => x.Type == AccountController.AUTH_TYPE))
                    {
                        try
                        {
                            AccountController.WriteJWT(new SignInDTO //-- dummy user
                                                           {
                                                               Login = "abc",
                                                               Name = "Otakar",
                                                               Password = "pwd",
                                                               Role = "borec"
                                                           }, context);
                            
                            Console.WriteLine("Autentizovan pres SSO");
                            context.Response.Redirect(context.Request.Path); // redirect na stejnou PATH, aby probehlo zapsani cookies
                        }
                        catch
                        {
                            AccountController.DeleteJWT(context);
                            context.Response.Redirect("/Account/SignIn"); // uzivatel neni v DB a nejedna se pouze o zobrazeni hesla
                        }
                        return; // autentizace bud probehla, nebo ne
                    }

                    // jiz se jedna o nasi autentizaci nebo prihlasovaci dialog
                    Console.WriteLine("Autentizovan pomoci JWT");
                    await _next(context);
                    return;
                }
            }

            if (!signIn)
            {
                context.Response.Redirect("/Account/SignIn"); // uzivatel neni prihlaseny, je presmerovan na dialog pro prihlaseni
            }
            else
            {
                await _next(context); // pouze pokud se jedna o dialog pro prihlaseni
            }
        }
    }
}
