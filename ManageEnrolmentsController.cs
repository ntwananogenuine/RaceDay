using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Filters;
using raceday_mvc.Models;
using raceday_mvc.Services;

namespace raceday_mvc.Controllers
{//start of namespace

    //all the participants enrolled for an event, with their category and status (organisers)
    [RoleRequired("Organiser")]
    public class ManageEnrolmentsController : RaceDayController
    {//start of class

        public ManageEnrolmentsController(api_client api) : base(api) { }

        public async Task<IActionResult> Index(int? eventId)
        {//start of method
            var (events, response) = await load_my_events();
            if (response.session_ended) return back_to_login();
            if (!response.ok) flash_error(response.message());

            EnrolmentsPageModel model = new EnrolmentsPageModel
            {
                Picker = new EventPickerModel
                {
                    Title = "Choose an event to see who enrolled",
                    Controller = "ManageEnrolments",
                    Events = events
                }
            };

            if (eventId != null)
            {
                model.Event = events.FirstOrDefault(e => e.EventId == eventId);
                if (model.Event == null)
                {
                    flash_error("We could not find that event.");
                    return RedirectToAction("Index");
                }

                foreach (CategoryInfo category in await load_categories(model.Event.EventId))
                {
                    api_response list = await api.get("/api/categories/" + category.CategoryId + "/enrolments");
                    model.Groups.Add(new CategoryEnrolments
                    {
                        Category = category,
                        Enrolments = list.ok ? (list.read<List<EnrolmentInfo>>() ?? new List<EnrolmentInfo>())
                                             : new List<EnrolmentInfo>()
                    });
                }
            }
            return View(model);
        }//end of method

        [HttpPost]
        public async Task<IActionResult> Remove(int id, int eventId)
        {//start of method
            api_response result = await api.delete("/api/enrolments/" + id);
            if (result.session_ended) return back_to_login();

            if (result.ok) flash_ok("The enrolment was removed.");
            else flash_error(result.message());

            return RedirectToAction("Index", new { eventId });
        }//end of method

    }//end of class
}//end of namespace
