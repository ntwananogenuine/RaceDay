namespace raceday_api.Models
{//start of namespace

    //what is sent back to the client, the password hash is never included
    public record UserDto(int UserId, string FullName, string Email, string Role, DateTime CreatedAt)
    {
        public static UserDto From(User u) => new(u.UserId, u.FullName, u.Email, u.Role.RoleName, u.CreatedAt);
    }

    public record EventDto(int EventId, int OrganiserId, string Name, string? Description,
        DateTime EventDate, string Location, string EventType, decimal? DistanceKm)
    {
        public static EventDto From(Event e) => new(e.EventId, e.OrganiserId, e.Name, e.Description,
            e.EventDate, e.Location, e.EventType, e.DistanceKm);
    }

    public record CategoryDto(int CategoryId, int EventId, string Name, decimal DistanceKm, int MaxParticipants)
    {
        public static CategoryDto From(Category c) => new(c.CategoryId, c.EventId, c.Name, c.DistanceKm, c.MaxParticipants);
    }

    public record EnrolmentDto(int EnrolmentId, int ParticipantId, string ParticipantName, int CategoryId,
        string CategoryName, int EventId, string EventName, DateTime EnrolmentDate, string BibNumber, string Status)
    {
        public static EnrolmentDto From(Enrolment e) => new(e.EnrolmentId, e.ParticipantId, e.Participant.FullName,
            e.CategoryId, e.Category.Name, e.Category.EventId, e.Category.Event.Name,
            e.EnrolmentDate, e.BibNumber, e.Status);
    }

    public record ResultDto(int ResultId, int EnrolmentId, int CategoryId, string ParticipantName,
        string BibNumber, TimeSpan? FinishTime, int? Position, string Status)
    {
        public static ResultDto From(Result r) => new(r.ResultId, r.EnrolmentId, r.Enrolment.CategoryId,
            r.Enrolment.Participant.FullName, r.Enrolment.BibNumber, r.FinishTime, r.Position, r.Status);
    }

}//end of namespace
