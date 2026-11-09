namespace raceday_mvc.Models
{//start of namespace

    //these classes match the json the Part 2 API sends back

    public class UserInfo
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class EventInfo
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public string EventType { get; set; } = "";
        public decimal? DistanceKm { get; set; }
    }

    public class CategoryInfo
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public int MaxParticipants { get; set; }
    }

    public class EnrolmentInfo
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = "";
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public DateTime EnrolmentDate { get; set; }
        public string BibNumber { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public class ResultInfo
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public int CategoryId { get; set; }
        public string ParticipantName { get; set; } = "";
        public string BibNumber { get; set; } = "";
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string Status { get; set; } = "";
    }

}//end of namespace
