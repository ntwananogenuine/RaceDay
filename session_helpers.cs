namespace raceday_mvc.Services
{//start of namespace

    //the names of the things kept in the session
    public static class session_keys
    {
        public const string user_id = "user_id";
        public const string role = "role";
        public const string full_name = "full_name";
        public const string email = "email";
        public const string api_cookie = "api_cookie"; //the session cookie of the API
    }

    public static class session_helpers
    {//start of class
        public static bool is_logged_in(this ISession session)
        {
            return session.GetInt32(session_keys.user_id) != null;
        }

        public static string role(this ISession session)
        {
            return session.GetString(session_keys.role) ?? "";
        }

        public static string full_name(this ISession session)
        {
            return session.GetString(session_keys.full_name) ?? "";
        }
    }//end of class

}//end of namespace
