using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Filters;
using raceday_mvc.Models;
using raceday_mvc.Services;

namespace raceday_mvc.Controllers
{//start of namespace

    //full event information and enrolling (participants)
    [RoleRequired("Participant")]
    public class EventsController : RaceDayController
    {//start of class

        public EventsController(api_client api) : base(api) { }

        public async Task<IActionResult> Details(int id)
        {//start of method
            api_response event_response = await api.get("/api/events/" + id);
            if (event_response.session_ended) return back_to_login();

            EventInfo? ev = event_response.ok ? event_response.read<EventInfo>() : null;
            if (ev == null)
            {
                flash_error(event_response.ok ? "We could not find that event." : event_response.message());
                return RedirectToAction("Index", "Home");
            }

            List<CategoryInfo> categories = await load_categories(id);

            //the categories the participant already entered
            HashSet<int> enrolled = new HashSet<int>();
            api_response mine = await api.get("/api/users/me/enrolments");
            if (mine.ok)
            {
                foreach (EnrolmentInfo e in mine.read<List<EnrolmentInfo>>() ?? new List<EnrolmentInfo>())
                {
                    enrolled.Add(e.CategoryId);
                }
            }

            EventDetailsViewModel model = new EventDetailsViewModel
            {
                Event = ev,
                Categories = categories,
                EnrolledCategoryIds = enrolled
            };
            return View(model);
        }//end of method

        //the participant picks a category and enrols, the API records the enrolment immediately
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {//start of method
            api_response result = await api.post("/api/categories/" + categoryId + "/enrol");
            if (result.session_ended) return back_to_login();

            if (result.ok)
            {
                EnrolmentInfo? enrolment = result.read<EnrolmentInfo>();
                flash_ok("You are enrolled! Your bib number is " + (enrolment?.BibNumber ?? "ready") + ".");
            }
            else
            {
                flash_error(result.message());
            }
            return RedirectToAction("Details", new { id = eventId });
        }//end of method

    }//end of class
}//end of namespace
