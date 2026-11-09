using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Models;
using raceday_mvc.Services;

namespace raceday_mvc.Controllers
{//start of namespace
    public class HomeController : RaceDayController
    {//start of class

        public HomeController(api_client api) : base(api) { }

        //visitors see the landing page, organisers go to their dashboard,
        //participants see all the upcoming events
        public async Task<IActionResult> Index(string? q, string? type, string? location, DateTime? date_from)
        {//start of method
            ISession session = HttpContext.Session;

            if (!session.is_logged_in()) return View("Landing");
            if (session.role() == "Organiser") return RedirectToAction("Index", "Dashboard");

            var (events, response) = await load_events();
            if (response.session_ended) return back_to_login();
            if (!response.ok) flash_error(response.message());

            //only events that have not happened yet, then the filters
            DateTime start = date_from?.Date ?? DateTime.Today;
            IEnumerable<EventInfo> filtered = events.Where(e => e.EventDate.Date >= start);

            if (!string.IsNullOrWhiteSpace(q))
            {
                string text = q.Trim();
                filtered = filtered.Where(e => e.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
                    || (e.Description ?? "").Contains(text, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(type))
            {
                filtered = filtered.Where(e => e.EventType == type);
            }
            if (!string.IsNullOrWhiteSpace(location))
            {
                string place = location.Trim();
                filtered = filtered.Where(e => e.Location.Contains(place, StringComparison.OrdinalIgnoreCase));
            }

            HomeViewModel model = new HomeViewModel
            {
                Events = filtered.OrderBy(e => e.EventDate).ToList(),
                Q = q,
                Type = type,
                Location = location,
                From = date_from
            };
            return View(model);
        }//end of method

        public IActionResult AccessDenied()
        {//start of method
            return View();
        }//end of method

        //friendly page for errors and missing pages
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? code)
        {//start of method
            ErrorViewModel model = new ErrorViewModel { Code = code ?? 500 };

            if (model.Code == 404)
            {
                model.Title = "Page not found";
                model.Message = "We could not find the page you were looking for.";
            }
            else
            {
                model.Title = "Something went wrong";
                model.Message = "Something went wrong on our side. Please try again in a moment.";
            }
            return View(model);
        }//end of method

    }//end of class
}//end of namespace
