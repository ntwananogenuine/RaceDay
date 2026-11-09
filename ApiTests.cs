using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using raceday_api.database;
using Xunit;
using static raceday_tests.Helpers;

namespace raceday_tests;

public class AuthTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _f;
    public AuthTests(RaceDayFactory f) => _f = f;

    [Fact]
    public async Task Register_ValidData_Returns201()
    {
        var r = await _f.CreateClient().PostParamsAsync("/api/auth/register",
            new { fullname = "Ann", email = NewEmail(), password = Password, role = "Participant" });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var c = _f.CreateClient();
        var body = new { fullname = "Ann", email = NewEmail(), password = Password, role = "Participant" };
        await c.PostParamsAsync("/api/auth/register", body);
        var r = await c.PostParamsAsync("/api/auth/register", body);
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidRole_Returns400()
    {
        var r = await _f.CreateClient().PostParamsAsync("/api/auth/register",
            new { fullname = "Ann", email = NewEmail(), password = Password, role = "Admin" });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400()
    {
        var r = await _f.CreateClient().PostParamsAsync("/api/auth/register",
            new { fullname = "Ann", email = NewEmail(), password = "short", role = "Participant" });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Register_StoresHashedPassword_NotPlainText()
    {
        var email = NewEmail();
        await _f.CreateClient().PostParamsAsync("/api/auth/register",
            new { fullname = "Ann", email, password = Password, role = "Participant" });

        using var scope = _f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = db.Users.Single(u => u.Email == email);
        Assert.NotEqual(Password, user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash));
    }

    [Fact]
    public async Task Login_CorrectCredentials_Returns200()
    {
        var c = _f.CreateClient();
        var email = NewEmail();
        await c.PostParamsAsync("/api/auth/register",
            new { fullname = "Ann", email, password = Password, role = "Organiser" });
        var r = await c.PostParamsAsync("/api/auth/login", new { email, password = Password });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var c = _f.CreateClient();
        var email = NewEmail();
        await c.PostParamsAsync("/api/auth/register",
            new { fullname = "Ann", email, password = Password, role = "Organiser" });
        var r = await c.PostParamsAsync("/api/auth/login", new { email, password = "WrongPass123" });
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Login_SessionKeepsUserLoggedIn()
    {
        var c = await LoggedInClient(_f, "Participant");
        var r = await c.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Logout_EndsSession()
    {
        var c = await LoggedInClient(_f, "Participant");
        await c.PostAsync("/api/auth/logout", null);
        var r = await c.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Theory]
    [InlineData("/api/users/me")]
    [InlineData("/api/users/me/enrolments")]
    [InlineData("/api/users/me/results")]
    public async Task ProtectedEndpoints_Unauthenticated_Return401(string url)
    {
        var r = await _f.CreateClient().GetAsync(url);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_Unauthenticated_Returns401()
    {
        var r = await _f.CreateClient().PostParamsAsync("/api/events", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_ChangesName()
    {
        var c = await LoggedInClient(_f, "Participant");
        var r = await c.PutParamsAsync("/api/users/me", new { fullname = "New Name", email = NewEmail() });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var me = await c.GetFromJsonAsync<JsonElement>("/api/users/me");
        Assert.Equal("New Name", me.GetProperty("fullName").GetString());
    }
}

public class EventTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _f;
    public EventTests(RaceDayFactory f) => _f = f;

    [Fact]
    public async Task Organiser_CanCreateEvent_Returns201()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var r = await org.PostParamsAsync("/api/events", new
        {
            name = "City Run", description = "d", event_date = DateTime.Today.AddDays(10),
            location = "JHB", event_type = "Run"
        });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
    }

    [Fact]
    public async Task Organiser_CreateEvent_InvalidType_Returns400()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var r = await org.PostParamsAsync("/api/events", new
        {
            name = "City Run", event_date = DateTime.Today.AddDays(10),
            location = "JHB", event_type = "Swim"
        });
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Participant_CreateEvent_Returns403()
    {
        var p = await LoggedInClient(_f, "Participant");
        var r = await p.PostParamsAsync("/api/events", new
        {
            name = "X", event_date = DateTime.Today.AddDays(10), location = "JHB", event_type = "Run"
        });
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanUpdateOwnEvent_Returns200()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var id = await CreateEvent(org);
        var r = await org.PutParamsAsync($"/api/events/{id}", new
        {
            name = "Renamed", event_date = DateTime.Today.AddDays(20), location = "Cape Town", event_type = "Cycle"
        });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotUpdateAnotherOrganisersEvent_Returns403()
    {
        var owner = await LoggedInClient(_f, "Organiser");
        var other = await LoggedInClient(_f, "Organiser");
        var id = await CreateEvent(owner);
        var r = await other.PutParamsAsync($"/api/events/{id}", new
        {
            name = "Hijack", event_date = DateTime.Today.AddDays(20), location = "X", event_type = "Run"
        });
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanDeleteOwnEvent_ThenItIsGone()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var id = await CreateEvent(org);
        Assert.Equal(HttpStatusCode.OK, (await org.DeleteAsync($"/api/events/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await org.GetAsync($"/api/events/{id}")).StatusCode);
    }

    [Fact]
    public async Task Participant_CannotDeleteEvent_Returns403()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var p = await LoggedInClient(_f, "Participant");
        var id = await CreateEvent(org);
        Assert.Equal(HttpStatusCode.Forbidden, (await p.DeleteAsync($"/api/events/{id}")).StatusCode);
    }

    [Fact]
    public async Task Events_PublicList_Returns200WithoutLogin()
    {
        var r = await _f.CreateClient().GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotAddCategory_Returns403()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var p = await LoggedInClient(_f, "Participant");
        var id = await CreateEvent(org);
        var r = await p.PostParamsAsync($"/api/events/{id}/categories",
            new { name = "5km", distance_km = 5, max_participants = 10 });
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanAddCategory_AndAnyoneCanListIt()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var id = await CreateEvent(org);
        await CreateCategory(org, id);
        var list = await _f.CreateClient().GetFromJsonAsync<JsonElement>($"/api/events/{id}/categories");
        Assert.Equal(1, list.GetArrayLength());
    }
}

public class EnrolmentTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _f;
    public EnrolmentTests(RaceDayFactory f) => _f = f;

    [Fact]
    public async Task Participant_CanEnrol_AndEnrolmentIsRecorded()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var p = await LoggedInClient(_f, "Participant");
        var catId = await CreateCategory(org, await CreateEvent(org));

        var r = await p.PostAsync($"/api/categories/{catId}/enrol", null);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);

        var mine = await p.GetFromJsonAsync<JsonElement>("/api/users/me/enrolments");
        Assert.Equal(1, mine.GetArrayLength());
        Assert.Equal(catId, mine[0].GetProperty("categoryId").GetInt32());
        Assert.False(string.IsNullOrEmpty(mine[0].GetProperty("bibNumber").GetString()));
    }

    [Fact]
    public async Task Participant_EnrolTwice_Returns409()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var p = await LoggedInClient(_f, "Participant");
        var catId = await CreateCategory(org, await CreateEvent(org));

        await p.PostAsync($"/api/categories/{catId}/enrol", null);
        var r = await p.PostAsync($"/api/categories/{catId}/enrol", null);
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
    }

    [Fact]
    public async Task Enrol_FullCategory_Returns409()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var p1 = await LoggedInClient(_f, "Participant");
        var p2 = await LoggedInClient(_f, "Participant");
        var catId = await CreateCategory(org, await CreateEvent(org), max: 1);

        Assert.Equal(HttpStatusCode.Created, (await p1.PostAsync($"/api/categories/{catId}/enrol", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await p2.PostAsync($"/api/categories/{catId}/enrol", null)).StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotEnrol_Returns403()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var catId = await CreateCategory(org, await CreateEvent(org));
        var r = await org.PostAsync($"/api/categories/{catId}/enrol", null);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Enrol_Unauthenticated_Returns401()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var catId = await CreateCategory(org, await CreateEvent(org));
        var r = await _f.CreateClient().PostAsync($"/api/categories/{catId}/enrol", null);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotViewCategoryEnrolments_Returns403()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var p = await LoggedInClient(_f, "Participant");
        var catId = await CreateCategory(org, await CreateEvent(org));
        var r = await p.GetAsync($"/api/categories/{catId}/enrolments");
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanViewEnrolmentsForOwnCategory()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var p = await LoggedInClient(_f, "Participant");
        var catId = await CreateCategory(org, await CreateEvent(org));
        await p.PostAsync($"/api/categories/{catId}/enrol", null);

        var list = await org.GetFromJsonAsync<JsonElement>($"/api/categories/{catId}/enrolments");
        Assert.Equal(1, list.GetArrayLength());
    }

    [Fact]
    public async Task Participant_CanCancelOwnEnrolment()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var p = await LoggedInClient(_f, "Participant");
        var catId = await CreateCategory(org, await CreateEvent(org));
        var en = await p.PostAsync($"/api/categories/{catId}/enrol", null);
        var enId = await Id(en, "enrolmentId");

        Assert.Equal(HttpStatusCode.OK, (await p.DeleteAsync($"/api/enrolments/{enId}")).StatusCode);
        var mine = await p.GetFromJsonAsync<JsonElement>("/api/users/me/enrolments");
        Assert.Equal(0, mine.GetArrayLength());
    }

    [Fact]
    public async Task Participant_CannotCancelSomeoneElsesEnrolment_Returns403()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var p1 = await LoggedInClient(_f, "Participant");
        var p2 = await LoggedInClient(_f, "Participant");
        var catId = await CreateCategory(org, await CreateEvent(org));
        var en = await p1.PostAsync($"/api/categories/{catId}/enrol", null);
        var enId = await Id(en, "enrolmentId");

        Assert.Equal(HttpStatusCode.Forbidden, (await p2.DeleteAsync($"/api/enrolments/{enId}")).StatusCode);
    }
}

public class ResultTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _f;
    public ResultTests(RaceDayFactory f) => _f = f;

    private static async Task<(HttpClient org, HttpClient p, int enrolmentId)> Setup(RaceDayFactory f)
    {
        var org = await LoggedInClient(f, "Organiser");
        var p = await LoggedInClient(f, "Participant");
        var catId = await CreateCategory(org, await CreateEvent(org));
        var en = await p.PostAsync($"/api/categories/{catId}/enrol", null);
        return (org, p, await Id(en, "enrolmentId"));
    }

    [Fact]
    public async Task Organiser_CanRecordResult_Returns201()
    {
        var (org, _, enId) = await Setup(_f);
        var r = await org.PostParamsAsync($"/api/enrolments/{enId}/result",
            new { finish_time = "00:45:10", position = 1 });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotRecordResult_Returns403()
    {
        var (_, p, enId) = await Setup(_f);
        var r = await p.PostParamsAsync($"/api/enrolments/{enId}/result",
            new { finish_time = "00:45:10", position = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task OtherOrganiser_CannotRecordResult_Returns403()
    {
        var (_, _, enId) = await Setup(_f);
        var other = await LoggedInClient(_f, "Organiser");
        var r = await other.PostParamsAsync($"/api/enrolments/{enId}/result",
            new { finish_time = "00:45:10", position = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task RecordResult_Twice_Returns409()
    {
        var (org, _, enId) = await Setup(_f);
        var body = new { finish_time = "00:45:10", position = 1 };
        await org.PostParamsAsync($"/api/enrolments/{enId}/result", body);
        var r = await org.PostParamsAsync($"/api/enrolments/{enId}/result", body);
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
    }

    [Fact]
    public async Task Participant_CanViewOwnResults()
    {
        var (org, p, enId) = await Setup(_f);
        await org.PostParamsAsync($"/api/enrolments/{enId}/result",
            new { finish_time = "00:45:10", position = 1 });

        var mine = await p.GetFromJsonAsync<JsonElement>("/api/users/me/results");
        Assert.Equal(1, mine.GetArrayLength());
        Assert.Equal(1, mine[0].GetProperty("position").GetInt32());
    }

    [Fact]
    public async Task Organiser_CanUpdateResult_Returns200()
    {
        var (org, _, enId) = await Setup(_f);
        var created = await org.PostParamsAsync($"/api/enrolments/{enId}/result",
            new { finish_time = "00:45:10", position = 2 });
        var resultId = await Id(created, "resultId");

        var r = await org.PutParamsAsync($"/api/results/{resultId}",
            new { finish_time = "00:44:00", position = 1 });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }
}
