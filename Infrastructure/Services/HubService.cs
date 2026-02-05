using Core.Interfaces;
using Infrastructure.Extensions;
using Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class HubService : Hub, IHubService
    {
        public string GetUserId(ClaimsPrincipal user)
        {
            return user?.GetMemberId() ?? throw new HubException("Cannot get member id");
        }
    }
}
