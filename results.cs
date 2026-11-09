namespace raceday_api.Models
{//start of namespace
    public class Result
    {//start of class
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string Status { get; set; } = "Pending";
        public Enrolment Enrolment { get; set; } = null!;
    }//end of class
}//end of namespace
