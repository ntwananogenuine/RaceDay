using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Models;
using raceday_mvc.Services;

namespace raceday_mvc.Controllers
{//start of namespace

    //shared by all the controllers
    public abstract class RaceDayController : Controller
    {//start of class

        protected readonly api_client api;

        protected RaceDayController(api_client api)
        {
            this.api = api;
        }

        //id of the user who is logged in (kept in the session)
        protected int CurrentUserId
        {
            get { return HttpContext.Session.GetInt32(session_keys.user_id) ?? 0; }
        }

        //messages shown at the top of the next page
        protected void flash_ok(string message) { TempData["ok"] = message; }
        protected void flash_warn(string message) { TempData["warn"] = message; }
        protected void flash_error(string message) { TempData["error"] = message; }

        //the API says the session ended, so the user has to log in again
        protected IActionResult back_to_login()
        {//start of method
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account", new { expired = 1 });
        }//end of method

        //gets all the events from the API
        protected async Task<(List<EventInfo> events, api_response response)> load_events()
        {//start of method
            api_response response = await api.get("/api/events");
            List<EventInfo> events = response.ok ? (response.read<List<EventInfo>>() ?? new List<EventInfo>())
                                                 : new List<EventInfo>();
            return (events, response);
        }//end of method

        //gets the events of the logged in organiser
        protected async Task<(List<EventInfo> events, api_response response)> load_my_events()
        {//start of method
            var (all, response) = await load_events();
            List<EventInfo> mine = all.Where(e => e.OrganiserId == CurrentUserId)
                                      .OrderBy(e => e.EventDate).ToList();
            return (mine, response);
        }//end of method

        //gets the categories of an event
        protected async Task<List<CategoryInfo>> load_categories(int event_id)
        {//start of method
            api_response response = await api.get("/api/events/" + event_id + "/categories");
            return response.ok ? (response.read<List<CategoryInfo>>() ?? new List<CategoryInfo>())
                               : new List<CategoryInfo>();
        }//end of method

    }//end of class
}//end of namespace
