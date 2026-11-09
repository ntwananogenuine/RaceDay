using System.Net;
using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Models;
using raceday_mvc.Services;

namespace raceday_mvc.Controllers
{//start of namespace
    public class AccountController : RaceDayController
    {//start of class

        public AccountController(api_client api) : base(api) { }

        [HttpGet]
        public IActionResult Login(int? expired)
        {//start of method
            if (HttpContext.Session.is_logged_in()) return RedirectToAction("Index", "Home");

            if (expired == 1 && TempData["error"] == null && TempData["ok"] == null)
            {
                ViewBag.Notice = "Please log in to continue.";
            }
            return View(new LoginForm());
        }//end of method

        [HttpPost]
        public async Task<IActionResult> Login(LoginForm form)
        {//start of method
            if (!ModelState.IsValid) return View(form);

            //the API checks the email and password
            api_response result = await api.post("/api/auth/login", new Dictionary<string, string?>
            {
                ["email"] = form.Email,
                ["password"] = form.Password
            });

            if (!result.ok)
            {
                string message = result.status == HttpStatusCode.Unauthorized
                    ? "Incorrect email or password."
                    : result.message();
                ModelState.AddModelError("", message);
                return View(form);
            }

            UserInfo? user = result.read<UserInfo>();
            if (user == null || result.set_cookie == null)
            {
                ModelState.AddModelError("", "We could not start your session. Please try again.");
                return View(form);
            }

            //remember who is logged in (and the API session cookie) in the session
            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32(session_keys.user_id, user.UserId);
            HttpContext.Session.SetString(session_keys.role, user.Role);
            HttpContext.Session.SetString(session_keys.full_name, user.FullName);
            HttpContext.Session.SetString(session_keys.email, user.Email);
            HttpContext.Session.SetString(session_keys.api_cookie, result.set_cookie);

            return RedirectToAction("Index", "Home");
        }//end of method

        [HttpGet]
        public IActionResult Register()
        {//start of method
            if (HttpContext.Session.is_logged_in()) return RedirectToAction("Index", "Home");
            return View(new RegisterForm());
        }//end of method

        [HttpPost]
        public async Task<IActionResult> Register(RegisterForm form)
        {//start of method
            if (!ModelState.IsValid) return View(form);

            api_response result = await api.post("/api/auth/register", new Dictionary<string, string?>
            {
                ["fullname"] = form.FullName,
                ["email"] = form.Email,
                ["password"] = form.Password,
                ["role"] = form.Role
            });

            if (!result.ok)
            {
                ModelState.AddModelError("", result.message());
                return View(form);
            }

            flash_ok("Your account is ready. Please log in.");
            return RedirectToAction("Login");
        }//end of method

        //ends the session and sends the user back to the login page
        [HttpPost]
        public async Task<IActionResult> Logout()
        {//start of method
            await api.post("/api/auth/logout");
            HttpContext.Session.Clear();
            flash_ok("You have been logged out.");
            return RedirectToAction("Login");
        }//end of method

    }//end of class
}//end of namespace
