using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Filters;
using raceday_mvc.Models;
using raceday_mvc.Services;

namespace raceday_mvc.Controllers
{//start of namespace

    //view and update personal information and the profile picture (both roles)
    [RoleRequired]
    public class ProfileController : RaceDayController
    {//start of class

        public ProfileController(api_client api) : base(api) { }

        public async Task<IActionResult> Index()
        {//start of method
            api_response response = await api.get("/api/users/me");
            if (response.session_ended) return back_to_login();

            UserInfo? user = response.ok ? response.read<UserInfo>() : null;
            if (user == null)
            {
                flash_error(response.message());
                return RedirectToAction("Index", "Home");
            }
            return View(user);
        }//end of method

        [HttpPost]
        public async Task<IActionResult> Update(string fullName, string email)
        {//start of method
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
            {
                flash_error("Please fill in your name and email.");
                return RedirectToAction("Index");
            }

            api_response result = await api.put("/api/users/me", new Dictionary<string, string?>
            {
                ["fullname"] = fullName,
                ["email"] = email
            });
            if (result.session_ended) return back_to_login();

            if (result.ok)
            {
                UserInfo? user = result.read<UserInfo>();
                if (user != null)
                {
                    HttpContext.Session.SetString(session_keys.full_name, user.FullName);
                    HttpContext.Session.SetString(session_keys.email, user.Email);
                }
                flash_ok("Your profile was updated.");
            }
            else
            {
                flash_error(result.message());
            }
            return RedirectToAction("Index");
        }//end of method

        //the picture goes to the API, which stores it in Azure Blob Storage
        [HttpPost]
        public async Task<IActionResult> Picture(IFormFile? picture)
        {//start of method
            if (picture == null || picture.Length == 0)
            {
                flash_error("Please choose an image to upload.");
                return RedirectToAction("Index");
            }

            api_response result = await api.post_file("/api/users/me/picture", picture);
            if (result.session_ended) return back_to_login();

            if (result.ok) flash_ok("Your profile picture was updated.");
            else flash_error(result.message());

            return RedirectToAction("Index");
        }//end of method

    }//end of class
}//end of namespace
