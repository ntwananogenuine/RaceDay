using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Filters;
using raceday_mvc.Models;
using raceday_mvc.Services;

namespace raceday_mvc.Controllers
{//start of namespace

    //summary for the organiser: events, enrolments per event and upcoming dates
    [RoleRequired("Organiser")]
    public class DashboardController : RaceDayController
    {//start of class

        public DashboardController(api_client api) : base(api) { }

        public async Task<IActionResult> Index()
        {//start of method
            var (events, response) = await load_my_events();
            if (response.session_ended) return back_to_login();
            if (!response.ok) flash_error(response.message());

            DashboardViewModel model = new DashboardViewModel();

            foreach (EventInfo ev in events)
            {
                List<CategoryInfo> categories = await load_categories(ev.EventId);

                //count the enrolments of every category of the event
                int enrolments = 0;
                foreach (CategoryInfo category in categories)
                {
                    api_response list = await api.get("/api/categories/" + category.CategoryId + "/enrolments");
                    if (list.ok)
                    {
                        enrolments += (list.read<List<EnrolmentInfo>>() ?? new List<EnrolmentInfo>()).Count;
                    }
                }

                model.Events.Add(new EventSummary
                {
                    Event = ev,
                    CategoryCount = categories.Count,
                    EnrolmentCount = enrolments
                });
            }

            model.TotalEnrolments = model.Events.Sum(e => e.EnrolmentCount);
            model.UpcomingCount = model.Events.Count(e => e.Event.EventDate.Date >= DateTime.Today);
            model.NextEventDate = model.Events.Where(e => e.Event.EventDate.Date >= DateTime.Today)
                                              .Select(e => (DateTime?)e.Event.EventDate)
                                              .OrderBy(d => d).FirstOrDefault();
            return View(model);
        }//end of method

    }//end of class
}//end of namespace
