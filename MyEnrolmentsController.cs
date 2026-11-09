using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Filters;
using raceday_mvc.Models;
using raceday_mvc.Services;

namespace raceday_mvc.Controllers
{//start of namespace

    //the events the participant enrolled for, with the enrolment status
    [RoleRequired("Participant")]
    public class MyEnrolmentsController : RaceDayController
    {//start of class

        public MyEnrolmentsController(api_client api) : base(api) { }

        public async Task<IActionResult> Index()
        {//start of method
            api_response response = await api.get("/api/users/me/enrolments");
            if (response.session_ended) return back_to_login();
            if (!response.ok) flash_error(response.message());

            List<EnrolmentInfo> enrolments = response.ok
                ? (response.read<List<EnrolmentInfo>>() ?? new List<EnrolmentInfo>())
                : new List<EnrolmentInfo>();

            //the enrolment only knows the event name, so get the date from the events
            var (events, _) = await load_events();

            List<MyEnrolmentRow> rows = enrolments.Select(e => new MyEnrolmentRow
            {
                Enrolment = e,
                EventDate = events.FirstOrDefault(ev => ev.EventId == e.EventId)?.EventDate
            }).OrderBy(r => r.EventDate).ToList();

            return View(rows);
        }//end of method

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {//start of method
            api_response result = await api.delete("/api/enrolments/" + id);
            if (result.session_ended) return back_to_login();

            if (result.ok) flash_ok("Your enrolment was cancelled.");
            else flash_error(result.message());

            return RedirectToAction("Index");
        }//end of method

    }//end of class
}//end of namespace
