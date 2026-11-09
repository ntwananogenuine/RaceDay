namespace raceday_api.Models
{//start of namespace
    public class Role
    {//start of class
        public int RoleId { get; set; }
        public string RoleName { get; set; } = "";
        public List<User> Users { get; set; } = new();
    }//end of class
}//end of namespace
