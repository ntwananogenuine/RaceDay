namespace raceday_mvc.Models
{//start of namespace

    //what each page needs to show

    public class HomeViewModel
    {
        public List<EventInfo> Events { get; set; } = new();
        public string? Q { get; set; }
        public string? Type { get; set; }
        public string? Location { get; set; }
        public DateTime? From { get; set; }
    }

    public class EventDetailsViewModel
    {
        public EventInfo Event { get; set; } = new();
        public List<CategoryInfo> Categories { get; set; } = new();
        public HashSet<int> EnrolledCategoryIds { get; set; } = new();
    }

    public class MyEnrolmentRow
    {
        public EnrolmentInfo Enrolment { get; set; } = new();
        public DateTime? EventDate { get; set; }
    }

    public class MyResultRow
    {
        public string EventName { get; set; } = "";
        public DateTime? EventDate { get; set; }
        public string CategoryName { get; set; } = "";
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public int TotalFinishers { get; set; }
        public string Status { get; set; } = "";
    }

    public class EventSummary
    {
        public EventInfo Event { get; set; } = new();
        public int CategoryCount { get; set; }
        public int EnrolmentCount { get; set; }
    }

    public class DashboardViewModel
    {
        public List<EventSummary> Events { get; set; } = new();
        public int TotalEnrolments { get; set; }
        public int UpcomingCount { get; set; }
        public DateTime? NextEventDate { get; set; }
    }

    //the cards used to choose an event on the Categories, Enrolments and Results pages
    public class EventPickerModel
    {
        public string Title { get; set; } = "";
        public string Controller { get; set; } = "";
        public List<EventInfo> Events { get; set; } = new();
    }

    public class CategoriesPageModel
    {
        public EventPickerModel Picker { get; set; } = new();
        public EventInfo? Event { get; set; }
        public List<CategoryInfo> Categories { get; set; } = new();
    }

    public class CategoryEnrolments
    {
        public CategoryInfo Category { get; set; } = new();
        public List<EnrolmentInfo> Enrolments { get; set; } = new();
    }

    public class EnrolmentsPageModel
    {
        public EventPickerModel Picker { get; set; } = new();
        public EventInfo? Event { get; set; }
        public List<CategoryEnrolments> Groups { get; set; } = new();
    }

    public class ResultRow
    {
        public EnrolmentInfo Enrolment { get; set; } = new();
        public ResultInfo? Result { get; set; }
    }

    public class ResultsPageModel
    {
        public EventPickerModel Picker { get; set; } = new();
        public EventInfo? Event { get; set; }
        public List<CategoryInfo> Categories { get; set; } = new();
        public CategoryInfo? Selected { get; set; }
        public List<ResultRow> Rows { get; set; } = new();
    }

    public class ErrorViewModel
    {
        public int Code { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
    }

    //small helpers for showing values
    public static class fmt
    {//start of class
        public static string ordinal(int number)
        {//start of method
            int last_two = number % 100;
            if (last_two >= 11 && last_two <= 13) return number + "th";
            switch (number % 10)
            {
                case 1: return number + "st";
                case 2: return number + "nd";
                case 3: return number + "rd";
                default: return number + "th";
            }
        }//end of method

        public static string time(TimeSpan? value)
        {
            return value == null ? "-" : value.Value.ToString(@"hh\:mm\:ss");
        }
    }//end of class

}//end of namespace
