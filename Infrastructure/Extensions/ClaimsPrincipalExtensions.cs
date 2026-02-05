using System;
using System.Security.Claims;

namespace Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions 
{
    public static string GetMemberId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("No NameIdentifier claim found");
    }
}
