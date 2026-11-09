using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using raceday_api.Auth;
using raceday_api.database;
using raceday_api.Models;

namespace raceday_api.Controllers
{//start of namespace
    //event banners and profile pictures, all the files go through the api to Azure Blob Storage
    [ApiController]
    public class manage_images : api_base
    {//start of class

        private readonly AppDbContext _db;
        private readonly i_blob_storage _blobs;

        public manage_images(AppDbContext db, i_blob_storage blobs)
        {//start of constructor
            _db = db;
            _blobs = blobs;
        }//end of constructor

        /// <summary>Uploads (or replaces) the banner image of an event. Only the Organiser who owns the event can do this.</summary>
        /// <remarks>Parameters: id, file (JPG, PNG, GIF or WebP, max 5 MB). Returns 200, 400 for a bad file, 401, 403, 404, or 502/503 if the image storage is not available.</remarks>
        [HttpPost("api/events/{id:int}/banner")]
        [session_authorize(role_names.Organiser)]
        public async Task<IActionResult> upload_event_banner(int id, IFormFile file)
        {//start of method
            Event? ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserId != current_user_id) return StatusCode(403, "You can only change the banner of your own events.");

            string? problem = image_rules.check(file);
            if (problem != null) return BadRequest(problem);
            if (!_blobs.is_configured) return StatusCode(503, "Image storage is not set up yet.");

            string new_name = "banners/" + id + "-" + Guid.NewGuid().ToString("N") + image_rules.extension(file.ContentType);
            string? old_name = ev.BannerImage;

            //try and catch
            try
            {//start of try
                using (Stream stream = file.OpenReadStream())
                {
                    await _blobs.upload(new_name, stream, file.ContentType);
                }
                ev.BannerImage = new_name;
                try { await _db.SaveChangesAsync(); }
                catch
                {
                    //Do not retain a new image that the database does not reference.
                    try { await _blobs.delete(new_name); } catch { /* cleanup best effort */ }
                    throw;
                }

                //A failed cleanup must not turn a successful upload into an error.
                if (old_name != null)
                {
                    try { await _blobs.delete(old_name); } catch { /* cleanup best effort */ }
                }
            }//end of try
            catch (Exception)
            {//start of catch
                return StatusCode(502, "The image could not be saved to storage. Please try again.");
            }//end of catch

            return Ok("Banner image saved.");
        }//end of method

        /// <summary>Gets the banner image of an event (anyone can view).</summary>
        /// <remarks>Parameter: id. Returns the image, or 404 if the event has no banner.</remarks>
        [HttpGet("api/events/{id:int}/banner")]
        public async Task<IActionResult> get_event_banner(int id)
        {//start of method
            Event? ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null || ev.BannerImage == null) return NotFound("This event has no banner image.");
            return await stream_blob(ev.BannerImage);
        }//end of method

        /// <summary>Uploads (or replaces) the profile picture of the logged in user.</summary>
        /// <remarks>Parameter: file (JPG, PNG, GIF or WebP, max 5 MB). Returns 200, 400 for a bad file, 401, or 502/503 if the image storage is not available.</remarks>
        [HttpPost("api/users/me/picture")]
        [session_authorize]
        public async Task<IActionResult> upload_profile_picture(IFormFile file)
        {//start of method
            User? user = await _db.Users.FindAsync(current_user_id);
            if (user == null) return Unauthorized();

            string? problem = image_rules.check(file);
            if (problem != null) return BadRequest(problem);
            if (!_blobs.is_configured) return StatusCode(503, "Image storage is not set up yet.");

            string new_name = "profiles/" + user.UserId + "-" + Guid.NewGuid().ToString("N") + image_rules.extension(file.ContentType);
            string? old_name = user.ProfilePicture;

            try
            {//start of try
                using (Stream stream = file.OpenReadStream())
                {
                    await _blobs.upload(new_name, stream, file.ContentType);
                }
                user.ProfilePicture = new_name;
                try { await _db.SaveChangesAsync(); }
                catch
                {
                    try { await _blobs.delete(new_name); } catch { /* cleanup best effort */ }
                    throw;
                }

                if (old_name != null)
                {
                    try { await _blobs.delete(old_name); } catch { /* cleanup best effort */ }
                }
            }//end of try
            catch (Exception)
            {//start of catch
                return StatusCode(502, "The image could not be saved to storage. Please try again.");
            }//end of catch

            return Ok("Profile picture saved.");
        }//end of method

        /// <summary>Gets the profile picture of the logged in user.</summary>
        /// <remarks>No parameters. Returns the image, 401, or 404 if no picture was uploaded yet.</remarks>
        [HttpGet("api/users/me/picture")]
        [session_authorize]
        public async Task<IActionResult> get_profile_picture()
        {//start of method
            User? user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == current_user_id);
            if (user == null) return Unauthorized();
            if (user.ProfilePicture == null) return NotFound("No profile picture yet.");
            return await stream_blob(user.ProfilePicture);
        }//end of method

        //reads an image from Azure Blob Storage and sends it back
        private async Task<IActionResult> stream_blob(string blob_name)
        {//start of method
            if (!_blobs.is_configured) return StatusCode(503, "Image storage is not set up yet.");

            try
            {//start of try
                var found = await _blobs.download(blob_name);
                if (found == null) return NotFound("The image was not found in storage.");
                return File(found.Value.content, found.Value.content_type);
            }//end of try
            catch (Exception)
            {//start of catch
                return StatusCode(502, "The image could not be read from storage.");
            }//end of catch
        }//end of method

    }//end of class
}//end of namespace
