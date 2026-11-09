using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace raceday_mvc.Services
{//start of namespace

    //what the API sent back
    public class api_response
    {//start of class
        public HttpStatusCode status { get; set; }
        public bool unreachable { get; set; }          //the API could not be reached at all
        public string content_type { get; set; } = "";
        public byte[] bytes { get; set; } = Array.Empty<byte>();
        public string? set_cookie { get; set; }        //the API session cookie (only sent at login)

        public bool ok => !unreachable && (int)status >= 200 && (int)status < 300;
        public bool session_ended => !unreachable && status == HttpStatusCode.Unauthorized;

        public string text => content_type.StartsWith("image/") ? "" : Encoding.UTF8.GetString(bytes);

        private static readonly JsonSerializerOptions json_options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        //turn the json into one of our classes
        public T? read<T>()
        {//start of method
            try
            {
                return JsonSerializer.Deserialize<T>(text, json_options);
            }
            catch (JsonException)
            {
                return default;
            }
        }//end of method

        //a message that is safe to show to the user (never a raw error)
        public string message()
        {//start of method
            if (unreachable)
                return "We could not reach the RaceDay service. Please try again in a moment.";

            int code = (int)status;
            if (code == 401)
                return "Your session has ended. Please log in again.";

            //the API sends short plain text messages for things like "Email already registered."
            bool plain = content_type.StartsWith("text/plain") && text.Length > 0 && text.Length < 200;
            if (plain && (code == 400 || code == 403 || code == 404 || code == 409 || code == 502 || code == 503))
                return text;

            if (code == 403) return "You do not have permission to do that.";
            if (code == 404) return "We could not find what you were looking for.";
            if (code >= 500) return "Something went wrong on our side. Please try again.";
            return "Please check the information you entered and try again.";
        }//end of method
    }//end of class

    //sends the requests to the Part 2 API
    public class api_client
    {//start of class

        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _context;

        public api_client(HttpClient http, IHttpContextAccessor context)
        {//start of constructor
            _http = http;
            _context = context;
        }//end of constructor

        //adds ?name=value&... to the url (values that are null are left out)
        private static string build_url(string url, Dictionary<string, string?>? query)
        {//start of method
            if (query == null) return url;
            Dictionary<string, string?> clean = query.Where(p => p.Value != null)
                                                     .ToDictionary(p => p.Key, p => p.Value);
            return QueryHelpers.AddQueryString(url, clean);
        }//end of method

        private async Task<api_response> send(HttpMethod method, string url, HttpContent? content = null)
        {//start of method
            try
            {//start of try
                using HttpRequestMessage request = new HttpRequestMessage(method, url) { Content = content };

                //send the API session cookie of the logged in user, so the API knows who is calling
                string? cookie = _context.HttpContext?.Session.GetString(session_keys.api_cookie);
                if (!string.IsNullOrEmpty(cookie))
                {
                    request.Headers.TryAddWithoutValidation("Cookie", cookie);
                }

                using HttpResponseMessage response = await _http.SendAsync(request);

                api_response result = new api_response
                {
                    status = response.StatusCode,
                    content_type = response.Content.Headers.ContentType?.MediaType ?? "",
                    bytes = await response.Content.ReadAsByteArrayAsync()
                };

                //at login the API gives us its session cookie, keep it
                if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies))
                {
                    string? session_cookie = cookies.FirstOrDefault(c => c.StartsWith(".AspNetCore.Session=", StringComparison.Ordinal));
                    result.set_cookie = session_cookie?.Split(';')[0];
                }

                return result;
            }//end of try
            catch (HttpRequestException)
            {//start of catch
                return new api_response { unreachable = true };
            }//end of catch
            catch (TaskCanceledException)
            {//start of catch
                return new api_response { unreachable = true };
            }//end of catch
        }//end of method

        public Task<api_response> get(string url, Dictionary<string, string?>? query = null)
        {
            return send(HttpMethod.Get, build_url(url, query));
        }

        public Task<api_response> post(string url, Dictionary<string, string?>? query = null)
        {
            return send(HttpMethod.Post, build_url(url, query));
        }

        public Task<api_response> put(string url, Dictionary<string, string?>? query = null)
        {
            return send(HttpMethod.Put, build_url(url, query));
        }

        public Task<api_response> delete(string url)
        {
            return send(HttpMethod.Delete, url);
        }

        //uploads an image to the API (the API stores it in Azure Blob Storage)
        public async Task<api_response> post_file(string url, IFormFile file)
        {//start of method
            using MultipartFormDataContent form = new MultipartFormDataContent();
            using Stream stream = file.OpenReadStream();
            StreamContent file_content = new StreamContent(stream);
            file_content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            form.Add(file_content, "file", file.FileName);

            return await send(HttpMethod.Post, url, form);
        }//end of method

    }//end of class
}//end of namespace
