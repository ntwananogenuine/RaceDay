using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Filters;
using raceday_mvc.Models;
using raceday_mvc.Services;

namespace raceday_mvc.Controllers
{//start of namespace

    //the race history of the participant
    [RoleRequired("Participant")]
    public class MyResultsController : RaceDayController
    {//start of class

        public MyResultsController(api_client api) : base(api) { }

        public async Task<IActionResult> Index()
        {//start of method
            api_response results_response = await api.get("/api/users/me/results");
            if (results_response.session_ended) return back_to_login();
            if (!results_response.ok) flash_error(results_response.message());

            List<ResultInfo> results = results_response.ok
                ? (results_response.read<List<ResultInfo>>() ?? new List<ResultInfo>())
                : new List<ResultInfo>();

            //the result does not hold the event name or date, get them from the enrolments and events
            api_response enrolments_response = await api.get("/api/users/me/enrolments");
            List<EnrolmentInfo> enrolments = enrolments_response.ok
                ? (enrolments_response.read<List<EnrolmentInfo>>() ?? new List<EnrolmentInfo>())
                : new List<EnrolmentInfo>();
            var (events, _) = await load_events();

            //how many people finished in each category (for "47th out of 312 finishers")
            Dictionary<int, int> finishers = new Dictionary<int, int>();
            foreach (int category_id in results.Select(r => r.CategoryId).Distinct())
            {
                api_response board = await api.get("/api/categories/" + category_id + "/results");
                List<ResultInfo> list = board.ok ? (board.read<List<ResultInfo>>() ?? new List<ResultInfo>())
                                                 : new List<ResultInfo>();
                finishers[category_id] = list.Count(r => r.Position != null);
            }

            List<MyResultRow> rows = new List<MyResultRow>();
            foreach (ResultInfo result in results)
            {
                EnrolmentInfo? enrolment = enrolments.FirstOrDefault(e => e.EnrolmentId == result.EnrolmentId);
                EventInfo? ev = events.FirstOrDefault(e => e.EventId == enrolment?.EventId);

                rows.Add(new MyResultRow
                {
                    EventName = enrolment?.EventName ?? "Event",
                    EventDate = ev?.EventDate,
                    CategoryName = enrolment?.CategoryName ?? "",
                    FinishTime = result.FinishTime,
                    Position = result.Position,
                    TotalFinishers = finishers.ContainsKey(result.CategoryId) ? finishers[result.CategoryId] : 0,
                    Status = result.Status
                });
            }

            return View(rows.OrderByDescending(r => r.EventDate).ToList());
        }//end of method

    }//end of class
}//end of namespace
