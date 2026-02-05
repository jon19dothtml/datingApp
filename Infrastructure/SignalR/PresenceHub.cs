using System;
using System.Security.Claims;
using Core.Interfaces;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR;

[Authorize]
public class PresenceHub(PresenceTracker presenceTracker, IHubService hubService ) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = hubService.GetUserId(Context.User!);
        await presenceTracker.UserConnected(userId, Context.ConnectionId);
        //notifichiamo gli altri utenti che siamo online
        await Clients.Others.SendAsync("UserOnline", userId); //questo è il metodo per il quale il client sarà in ascolto per essere notificato quando uno user è online
        var currentUsers= await presenceTracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        //notifichiamo gli altri utenti che ci stiamo disconnettendo
        await presenceTracker.UserDisconnected(hubService.GetUserId(Context.User!), Context.ConnectionId);
        await Clients.Others.SendAsync("UserOffline", hubService.GetUserId(Context.User!));

        var currentUsers= await presenceTracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);

        await base.OnDisconnectedAsync(exception);
    }

    
}
