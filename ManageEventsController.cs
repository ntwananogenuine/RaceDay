using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Filters;
using raceday_mvc.Models;
using raceday_mvc.Services;

namespace raceday_mvc.Controllers
{//start of namespace

    //create, edit and delete events (organisers)
    [RoleRequired("Organiser")]
    public class ManageEventsController : RaceDayController
    {//start of class

        public ManageEventsController(api_client api) : base(api) { }

        public async Task<IActionResult> Index()
        {//start of method
            var (events, response) = await load_my_events();
            if (response.session_ended) return back_to_login();
            if (!response.ok) flash_error(response.message());
            return View(events);
        }//end of method

        //the same form is used to create (no id) and to edit (with id)
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {//start of method
            if (id == null) return View(new EventForm());

            var (events, response) = await load_my_events();
            if (response.session_ended) return back_to_login();

            EventInfo? ev = events.FirstOrDefault(e => e.EventId == id);
            if (ev == null)
            {
                flash_error("We could not find that event.");
                return RedirectToAction("Index");
            }

            return View(new EventForm
            {
                EventId = ev.EventId,
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                EventType = ev.EventType,
                DistanceKm = ev.DistanceKm
            });
        }//end of method

        [HttpPost]
        public async Task<IActionResult> Edit(EventForm form)
        {//start of method
            if (!ModelState.IsValid) return View(form);

            Dictionary<string, string?> values = new Dictionary<string, string?>
            {
                ["name"] = form.Name,
                ["description"] = form.Description,
                ["event_date"] = form.EventDate!.Value.ToString("yyyy-MM-dd"),
                ["location"] = form.Location,
                ["event_type"] = form.EventType,
                ["distance_km"] = form.DistanceKm?.ToString(CultureInfo.InvariantCulture)
            };

            api_response result;
            int event_id;

            if (form.EventId == null)
            {
                result = await api.post("/api/events", values);
                event_id = result.ok ? (result.read<EventInfo>()?.EventId ?? 0) : 0;
            }
            else
            {
                result = await api.put("/api/events/" + form.EventId, values);
                event_id = form.EventId.Value;
            }

            if (result.session_ended) return back_to_login();
            if (!result.ok)
            {
                ModelState.AddModelError("", result.message());
                return View(form);
            }

            //the banner goes to the API, which stores it in Azure Blob Storage
            if (form.Banner != null && form.Banner.Length > 0 && event_id > 0)
            {
                api_response upload = await api.post_file("/api/events/" + event_id + "/banner", form.Banner);
                if (!upload.ok)
                {
                    flash_warn("The event was saved, but the banner was not: " + upload.message());
                    return RedirectToAction("Index");
                }
            }

            flash_ok(form.EventId == null ? "The event was created." : "The event was updated.");
            return RedirectToAction("Index");
        }//end of method

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {//start of method
            api_response result = await api.delete("/api/events/" + id);
            if (result.session_ended) return back_to_login();

            if (result.ok) flash_ok("The event was deleted.");
            else flash_error(result.message());

            return RedirectToAction("Index");
        }//end of method

    }//end of class
}//end of namespace
