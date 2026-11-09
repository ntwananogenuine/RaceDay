using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using raceday_api.database;

namespace raceday_tests;

public class RaceDayFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Swap SQL Server for an in-memory database so tests never touch LocalDB
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericArguments().Contains(typeof(AppDbContext)))).ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));

            // Swap Azure Blob Storage for a fake one kept in memory
            services.RemoveAll<i_blob_storage>();
            services.AddSingleton<i_blob_storage, fake_blob_storage>();
        });
    }

    public RaceDayFactory()
    {
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }
}

// Stands in for Azure Blob Storage during the tests
public class fake_blob_storage : i_blob_storage
{
    private readonly Dictionary<string, (byte[] data, string type)> _files = new();

    public bool is_configured => true;

    public async Task upload(string blob_name, Stream content, string content_type)
    {
        using var copy = new MemoryStream();
        await content.CopyToAsync(copy);
        lock (_files) { _files[blob_name] = (copy.ToArray(), content_type); }
    }

    public Task<(Stream content, string content_type)?> download(string blob_name)
    {
        lock (_files)
        {
            if (!_files.TryGetValue(blob_name, out var file))
                return Task.FromResult<(Stream content, string content_type)?>(null);
            return Task.FromResult<(Stream content, string content_type)?>((new MemoryStream(file.data), file.type));
        }
    }

    public Task delete(string blob_name)
    {
        lock (_files) { _files.Remove(blob_name); }
        return Task.CompletedTask;
    }
}

public static class Helpers
{
    public const string Password = "Password123!";

    public static string NewEmail() => $"{Guid.NewGuid():N}@test.com";

    // The API takes its values as parameters (query string), so the tests send them the same way.
    public static Task<HttpResponseMessage> PostParamsAsync(this HttpClient c, string url, object body) =>
        c.PostAsync(url + ToQuery(body), null);

    public static Task<HttpResponseMessage> PutParamsAsync(this HttpClient c, string url, object body) =>
        c.PutAsync(url + ToQuery(body), null);

    private static string ToQuery(object body)
    {
        var parts = body.GetType().GetProperties()
            .Where(p => p.GetValue(body) != null)
            .Select(p =>
            {
                var v = p.GetValue(body)!;
                var text = v is DateTime d
                    ? d.ToString("o", CultureInfo.InvariantCulture)
                    : Convert.ToString(v, CultureInfo.InvariantCulture)!;
                return $"{p.Name}={Uri.EscapeDataString(text)}";
            }).ToList();
        return parts.Count == 0 ? "" : "?" + string.Join("&", parts);
    }

    public static async Task<HttpClient> LoggedInClient(RaceDayFactory f, string role)
    {
        var client = f.CreateClient(); // each client keeps its own session cookie
        var email = NewEmail();
        (await client.PostParamsAsync("/api/auth/register",
            new { fullname = "Test User", email, password = Password, role })).EnsureSuccessStatusCode();
        (await client.PostParamsAsync("/api/auth/login",
            new { email, password = Password })).EnsureSuccessStatusCode();
        return client;
    }

    public static async Task<int> Id(HttpResponseMessage r, string prop)
    {
        var json = await r.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty(prop).GetInt32();
    }

    public static async Task<int> CreateEvent(HttpClient organiser)
    {
        var r = await organiser.PostParamsAsync("/api/events", new
        {
            name = "Test Run", description = "Fun", event_date = DateTime.Today.AddDays(30),
            location = "Pretoria", event_type = "Run", distance_km = 10
        });
        r.EnsureSuccessStatusCode();
        return await Id(r, "eventId");
    }

    public static async Task<int> CreateCategory(HttpClient organiser, int eventId, int max = 100)
    {
        var r = await organiser.PostParamsAsync($"/api/events/{eventId}/categories",
            new { name = "10km", distance_km = 10, max_participants = max });
        r.EnsureSuccessStatusCode();
        return await Id(r, "categoryId");
    }

    // A small fake image (PNG header) sent as a file upload
    public static MultipartFormDataContent ImageContent(string contentType = "image/png", byte[]? bytes = null)
    {
        bytes ??= new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48 };
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        var form = new MultipartFormDataContent();
        form.Add(file, "file", "image.png");
        return form;
    }
}
