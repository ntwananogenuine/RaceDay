namespace raceday_api.Models
{//start of namespace
    public class User
    {//start of class
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string? ProfilePicture { get; set; } //name of the picture in Azure Blob Storage
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Role Role { get; set; } = null!;
    }//end of class
}//end of namespace
