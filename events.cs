namespace raceday_api.Models
{//start of namespace
    public class Event
    {//start of class
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public string EventType { get; set; } = "Run"; // Run | Walk | Cycle
        public decimal? DistanceKm { get; set; }
        public string? BannerImage { get; set; } //name of the banner in Azure Blob Storage
        public User Organiser { get; set; } = null!;
        public List<Category> Categories { get; set; } = new();
    }//end of class
}//end of namespace
