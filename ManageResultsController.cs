using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Filters;
using raceday_mvc.Models;
using raceday_mvc.Services;

namespace raceday_mvc.Controllers
{//start of namespace

    //capture and publish finish times and positions after an event (organisers)
    [RoleRequired("Organiser")]
    public class ManageResultsController : RaceDayController
    {//start of class

        public ManageResultsController(api_client api) : base(api) { }

        public async Task<IActionResult> Index(int? eventId, int? categoryId)
        {//start of method
            var (events, response) = await load_my_events();
            if (response.session_ended) return back_to_login();
            if (!response.ok) flash_error(response.message());

            ResultsPageModel model = new ResultsPageModel
            {
                Picker = new EventPickerModel
                {
                    Title = "Choose an event to capture its results",
                    Controller = "ManageResults",
                    Events = events
                }
            };

            if (eventId == null) return View(model);

            model.Event = events.FirstOrDefault(e => e.EventId == eventId);
            if (model.Event == null)
            {
                flash_error("We could not find that event.");
                return RedirectToAction("Index");
            }

            model.Categories = await load_categories(model.Event.EventId);
            model.Selected = model.Categories.FirstOrDefault(c => c.CategoryId == categoryId)
                             ?? model.Categories.FirstOrDefault();

            if (model.Selected != null)
            {
                api_response enrolments_response = await api.get("/api/categories/" + model.Selected.CategoryId + "/enrolments");
                List<EnrolmentInfo> enrolments = enrolments_response.ok
                    ? (enrolments_response.read<List<EnrolmentInfo>>() ?? new List<EnrolmentInfo>())
                    : new List<EnrolmentInfo>();

                api_response results_response = await api.get("/api/categories/" + model.Selected.CategoryId + "/results");
                List<ResultInfo> results = results_response.ok
                    ? (results_response.read<List<ResultInfo>>() ?? new List<ResultInfo>())
                    : new List<ResultInfo>();

                model.Rows = enrolments.Select(e => new ResultRow
                {
                    Enrolment = e,
                    Result = results.FirstOrDefault(r => r.EnrolmentId == e.EnrolmentId)
                }).OrderBy(r => r.Result?.Position ?? int.MaxValue).ThenBy(r => r.Enrolment.BibNumber).ToList();
            }
            return View(model);
        }//end of method

        //saves (and publishes) the finish time and position of one participant
        [HttpPost]
        public async Task<IActionResult> Save(int eventId, int categoryId, int enrolmentId, int? resultId,
                                              string? finishTime, int? position)
        {//start of method
            string time_text = (finishTime ?? "").Trim();
            if (!Regex.IsMatch(time_text, @"^\d{1,2}:\d{2}:\d{2}$")
                || !TimeSpan.TryParse(time_text, CultureInfo.InvariantCulture, out TimeSpan time))
            {
                flash_error("Enter the finish time as hh:mm:ss, for example 00:45:10.");
                return RedirectToAction("Index", new { eventId, categoryId });
            }
            if (position == null || position < 1)
            {
                flash_error("Enter the finishing position as a whole number (1 or higher).");
                return RedirectToAction("Index", new { eventId, categoryId });
            }

            Dictionary<string, string?> values = new Dictionary<string, string?>
            {
                ["finish_time"] = time.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture),
                ["position"] = position.Value.ToString(CultureInfo.InvariantCulture)
            };

            //a new result, or an update of the one that was captured before
            api_response result = resultId == null
                ? await api.post("/api/enrolments/" + enrolmentId + "/result", values)
                : await api.put("/api/results/" + resultId, values);
            if (result.session_ended) return back_to_login();

            if (result.ok) flash_ok("The result was saved and published.");
            else flash_error(result.message());

            return RedirectToAction("Index", new { eventId, categoryId });
        }//end of method

    }//end of class
}//end of namespace
