using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Core.Interfaces
{
    public interface IHubService
    {
        string GetUserId(ClaimsPrincipal user);
    }
}
