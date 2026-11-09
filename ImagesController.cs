using Microsoft.AspNetCore.Mvc;
using raceday_mvc.Filters;
using raceday_mvc.Services;

namespace raceday_mvc.Controllers
{//start of namespace

    //shows the images the API has in Azure Blob Storage
    //the browser asks this project, this project asks the API (never Azure directly)
    public class ImagesController : RaceDayController
    {//start of class

        private readonly IWebHostEnvironment _env;

        public ImagesController(api_client api, IWebHostEnvironment env) : base(api)
        {
            _env = env;
        }

        //event banner, a placeholder is shown when the event has no banner
        [HttpGet]
        public async Task<IActionResult> Event(int id)
        {//start of method
            api_response response = await api.get("/api/events/" + id + "/banner");
            return response.ok ? image(response) : placeholder("banner-placeholder.svg");
        }//end of method

        //profile picture of the logged in user
        [HttpGet]
        [RoleRequired]
        public async Task<IActionResult> Profile()
        {//start of method
            api_response response = await api.get("/api/users/me/picture");
            return response.ok ? image(response) : placeholder("avatar-placeholder.svg");
        }//end of method

        private IActionResult image(api_response response)
        {//start of method
            Response.Headers["Cache-Control"] = "no-cache";
            return File(response.bytes, response.content_type);
        }//end of method

        private IActionResult placeholder(string file_name)
        {//start of method
            string path = Path.Combine(_env.WebRootPath, "images", file_name);
            return PhysicalFile(path, "image/svg+xml");
        }//end of method

    }//end of class
}//end of namespace
