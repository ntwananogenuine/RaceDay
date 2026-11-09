using System.Net;
using System.Text;
using Xunit;
using static raceday_tests.Helpers;

namespace raceday_tests;

public class ImageTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _f;
    public ImageTests(RaceDayFactory f) => _f = f;

    [Fact]
    public async Task Organiser_CanUploadBanner_AndAnyoneCanDownloadIt()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var id = await CreateEvent(org);

        var up = await org.PostAsync($"/api/events/{id}/banner", ImageContent());
        Assert.Equal(HttpStatusCode.OK, up.StatusCode);

        var get = await _f.CreateClient().GetAsync($"/api/events/{id}/banner");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Equal("image/png", get.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Participant_CannotUploadBanner_Returns403()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var p = await LoggedInClient(_f, "Participant");
        var id = await CreateEvent(org);

        var r = await p.PostAsync($"/api/events/{id}/banner", ImageContent());
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task OtherOrganiser_CannotUploadBanner_Returns403()
    {
        var owner = await LoggedInClient(_f, "Organiser");
        var other = await LoggedInClient(_f, "Organiser");
        var id = await CreateEvent(owner);

        var r = await other.PostAsync($"/api/events/{id}/banner", ImageContent());
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task UploadBanner_NotAnImage_Returns400()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var id = await CreateEvent(org);

        var r = await org.PostAsync($"/api/events/{id}/banner",
            ImageContent("text/plain", Encoding.UTF8.GetBytes("this is just text, not a picture")));
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task UploadBanner_Unauthenticated_Returns401()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var id = await CreateEvent(org);

        var r = await _f.CreateClient().PostAsync($"/api/events/{id}/banner", ImageContent());
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Banner_WhenNoneUploaded_Returns404()
    {
        var org = await LoggedInClient(_f, "Organiser");
        var id = await CreateEvent(org);

        var r = await _f.CreateClient().GetAsync($"/api/events/{id}/banner");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task User_CanUploadProfilePicture_AndGetItBack()
    {
        var p = await LoggedInClient(_f, "Participant");

        var up = await p.PostAsync("/api/users/me/picture", ImageContent());
        Assert.Equal(HttpStatusCode.OK, up.StatusCode);

        var get = await p.GetAsync("/api/users/me/picture");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task ProfilePicture_Unauthenticated_Returns401()
    {
        var r = await _f.CreateClient().GetAsync("/api/users/me/picture");
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }
}
