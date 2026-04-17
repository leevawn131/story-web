using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using story_web.Extensions;

namespace story_web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AuthAttribute : ActionFilterAttribute
{
    private readonly int[] _allowedRoles;

    public AuthAttribute(params int[] allowedRoles)
    {
        _allowedRoles = allowedRoles ?? [];
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetCurrentUserId();

        if (!userId.HasValue)
        {
            var request = context.HttpContext.Request;
            var returnUrl = $"{request.Path}{request.QueryString}";
            context.Result = new RedirectToActionResult("Login", "Account", new { area = "", returnUrl });
            return;
        }

        if (_allowedRoles.Length > 0)
        {
            var currentRole = session.GetCurrentUserRole();
            if (!currentRole.HasValue || !_allowedRoles.Contains(currentRole.Value))
            {
                context.Result = new RedirectToActionResult("Profile", "Account", new { area = "" });
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}
