using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Filters;
using raceday_mvc.Models;
using raceday_mvc.Services;

namespace raceday_mvc.Controllers
{//start of namespace

    //add age or distance categories to an event (organisers)
    [RoleRequired("Organiser")]
    public class ManageCategoriesController : RaceDayController
    {//start of class

        public ManageCategoriesController(api_client api) : base(api) { }

        //without an event: choose an event, with an event: its categories
        public async Task<IActionResult> Index(int? eventId)
        {//start of method
            var (events, response) = await load_my_events();
            if (response.session_ended) return back_to_login();
            if (!response.ok) flash_error(response.message());

            CategoriesPageModel model = new CategoriesPageModel
            {
                Picker = new EventPickerModel
                {
                    Title = "Choose an event to manage its categories",
                    Controller = "ManageCategories",
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
                model.Categories = await load_categories(model.Event.EventId);
            }
            return View(model);
        }//end of method

        [HttpPost]
        public async Task<IActionResult> Add(int eventId, string name, decimal distanceKm, int maxParticipants)
        {//start of method
            if (string.IsNullOrWhiteSpace(name))
            {
                flash_error("Please enter a category name, for example Under 20 or 10km.");
                return RedirectToAction("Index", new { eventId });
            }

            api_response result = await api.post("/api/events/" + eventId + "/categories", new Dictionary<string, string?>
            {
                ["name"] = name,
                ["distance_km"] = distanceKm.ToString(CultureInfo.InvariantCulture),
                ["max_participants"] = maxParticipants.ToString(CultureInfo.InvariantCulture)
            });
            if (result.session_ended) return back_to_login();

            if (result.ok) flash_ok("The category was added.");
            else flash_error(result.message());

            return RedirectToAction("Index", new { eventId });
        }//end of method

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int eventId)
        {//start of method
            api_response result = await api.delete("/api/categories/" + id);
            if (result.session_ended) return back_to_login();

            if (result.ok) flash_ok("The category was deleted.");
            else flash_error(result.message());

            return RedirectToAction("Index", new { eventId });
        }//end of method

    }//end of class
}//end of namespace
