namespace raceday_api.Models
{//start of namespace
    public class Category
    {//start of class
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public int MaxParticipants { get; set; } = 100;
        public Event Event { get; set; } = null!;
        public List<Enrolment> Enrolments { get; set; } = new();
    }//end of class
}//end of namespace
