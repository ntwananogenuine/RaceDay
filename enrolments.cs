namespace raceday_api.Models
{//start of namespace
    public class Enrolment
    {//start of class
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EnrolmentDate { get; set; } = DateTime.Now;
        public string BibNumber { get; set; } = "";
        public string Status { get; set; } = "Registered";
        public User Participant { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public Result? Result { get; set; }
    }//end of class
}//end of namespace
