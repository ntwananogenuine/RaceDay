using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using raceday_mvc.Services;

namespace raceday_mvc.Filters
{//start of namespace

    //keeps people out of pages they may not see:
    //not logged in = back to the login page, wrong role = access denied page
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleRequiredAttribute : Attribute, IAuthorizationFilter
    {//start of class
        private readonly string[] _roles;

        public RoleRequiredAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {//start of method
            ISession session = context.HttpContext.Session;

            if (!session.is_logged_in())
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { expired = 1 });
                return;
            }

            if (_roles.Length > 0 && !_roles.Contains(session.role()))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
            }
        }//end of method
    }//end of class

}//end of namespace
