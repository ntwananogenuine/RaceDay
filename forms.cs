using System.ComponentModel.DataAnnotations;

namespace raceday_mvc.Models
{//start of namespace

    //what the user types in on the forms

    public class LoginForm
    {
        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    public class RegisterForm
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100, ErrorMessage = "The name is too long.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please choose a password.")]
        [MinLength(8, ErrorMessage = "The password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Please repeat your password.")]
        [Compare("Password", ErrorMessage = "The passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = "";

        [Required(ErrorMessage = "Please choose your role.")]
        [Display(Name = "I am an")]
        public string Role { get; set; } = "";
    }

    public class EventForm
    {
        public int? EventId { get; set; }

        [Required(ErrorMessage = "Please enter the event name.")]
        [StringLength(150, ErrorMessage = "The name is too long (max 150 characters).")]
        public string Name { get; set; } = "";

        [StringLength(1000, ErrorMessage = "The description is too long (max 1000 characters).")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please choose the event date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime? EventDate { get; set; }

        [Required(ErrorMessage = "Please enter the location.")]
        [StringLength(150, ErrorMessage = "The location is too long (max 150 characters).")]
        public string Location { get; set; } = "";

        [Required(ErrorMessage = "Please choose the event type.")]
        [Display(Name = "Event type")]
        public string EventType { get; set; } = "";

        [Range(0.01, 999.99, ErrorMessage = "The distance must be between 0.01 and 999.99 km.")]
        [Display(Name = "Distance (km)")]
        public decimal? DistanceKm { get; set; }

        //the banner image (optional)
        public IFormFile? Banner { get; set; }
    }

}//end of namespace
