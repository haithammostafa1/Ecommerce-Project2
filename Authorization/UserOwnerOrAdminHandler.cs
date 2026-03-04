using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EcommrceApi.Authorization
{
    public class UserOwnerOrAdminHandler:AuthorizationHandler<UserOwnerOrAdminRequirement,int>
    {
        protected override  Task HandleRequirementAsync (AuthorizationHandlerContext context, UserOwnerOrAdminRequirement requirement, int resourceId)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            var userIdClaim =  context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim,out int authenticatedId) && authenticatedId == resourceId)
            {
                      
                    context.Succeed(requirement);
               
            }
            return Task.CompletedTask;
        }
    }
}
