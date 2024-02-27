using Group2_A3.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using System.Net.Http;
using System.Text;

namespace Group2_A3.Controllers
{
    public class LoginController : Controller
    {
        private eStore1Context context = new eStore1Context();
        private readonly IConfiguration configuration;


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("IsLoggedIn");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public IActionResult Login()
        {
            string email = HttpContext.Request.Form["email"];
            string password = HttpContext.Request.Form["password"];

            var builder = new ConfigurationBuilder()
                                            .SetBasePath(Directory.GetCurrentDirectory())
                                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            string fixedUsername = configuration.GetConnectionString("Username");
            string fixedPassword = configuration.GetConnectionString("Password");

            if (email.Equals(fixedUsername) && password.Equals(fixedPassword))
            {
                HttpContext.Session.SetString("IsLoggedIn", "true");
                HttpContext.Session.SetString("IsDefaultAdmin", "true");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Member? member = context.Members.FirstOrDefault(x => x.Email.Equals(email) && x.Password.Equals(password));
                if (member == null)
                {
                    TempData["ErrorMessage"] = "Invalid email or password.";
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    HttpContext.Session.SetString("IsLoggedIn", "true");
                    HttpContext.Session.SetString("IsDefaultAdmin", "false");
                    HttpContext.Session.SetInt32("LoggedInMemberId", member.MemberId);
                    return RedirectToAction("Index", "Home");
                }

            }


        }
    }
}
