using Microsoft.AspNetCore.Http;
using story_web.Models;

namespace story_web.Extensions;

public static class SessionExtensions
{
    public const string UserIdKey = "Auth.UserId";
    public const string UserNameKey = "Auth.UserName";
    public const string EmailKey = "Auth.Email";
    public const string RoleKey = "Auth.Role";

    public static void SetCurrentUser(this ISession session, User user)
    {
        session.SetInt32(UserIdKey, user.id_User);
        session.SetString(UserNameKey, user.UserName);
        session.SetString(EmailKey, user.Email);
        session.SetInt32(RoleKey, user.Role);
    }

    public static void ClearCurrentUser(this ISession session)
    {
        session.Remove(UserIdKey);
        session.Remove(UserNameKey);
        session.Remove(EmailKey);
        session.Remove(RoleKey);
    }

    public static int? GetCurrentUserId(this ISession session)
    {
        return session.GetInt32(UserIdKey);
    }

    public static string? GetCurrentUserName(this ISession session)
    {
        return session.GetString(UserNameKey);
    }

    public static string? GetCurrentUserEmail(this ISession session)
    {
        return session.GetString(EmailKey);
    }

    public static int? GetCurrentUserRole(this ISession session)
    {
        return session.GetInt32(RoleKey);
    }

    public static bool IsAuthenticated(this ISession session)
    {
        return session.GetCurrentUserId().HasValue;
    }
}
