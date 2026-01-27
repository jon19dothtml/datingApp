using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;

namespace API.SignalR;

[Authorize]
public class MessageHub(IMessageRepository messageRepository, IMemberRepository memberRepository, 
    IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext= Context.GetHttpContext(); //qui inizia la negoziazione principale
        var otherUser= httpContext?.Request?.Query["userId"].ToString()
             ?? throw new HubException("Other user not found");
        
        var groupName= GetGroupName(GetUserId(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await AddToGroup(groupName); //aggiungiamo al db il nome del group
        var messages= await messageRepository.GetMessageThread(GetUserId(), otherUser);

        await Clients.Group(groupName).SendAsync("RecieveMessageThread", messages);

    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var sender= await memberRepository.GetMemberByIdAsync(GetUserId());  //User.GetMemberId() è un’altra extension method che legge l’ID dell’utente loggato dal JWT
        var recipient= await memberRepository.GetMemberByIdAsync(createMessageDto.RecipientId); //prendiamo il destinatario dal dto che passiamo quando richiamiamo l'api POST
        if(sender == null || recipient== null || sender.Id == createMessageDto.RecipientId)
        {
            throw new HubException("Cannot send message");
        }
        var message= new Message
        {
            SenderId= sender.Id,
            RecipientId= recipient.Id,
            Content= createMessageDto.Content,
        };

        var groupName= GetGroupName(sender.Id, recipient.Id);
        var group= await messageRepository.GetMessageGroup(groupName);
        var userInGroup= group != null && group.Connections.Any(x => x.UserId == message.RecipientId);

        if(userInGroup) //controlliamo se user e recipient sono in questo gruppo
        {
            message.DateRead = DateTime.UtcNow; 
        }
        messageRepository.AddMessage(message);
        if(await messageRepository.SaveAllAsync())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", message.ToDto());
            var connections= await PresenceTracker.GetConnectionsForUser(recipient.Id);
            if(connections != null && connections.Count >0 && !userInGroup)
            {
                await presenceHub.Clients.Clients(connections) //usiamo due volte Clients per specificare chi è il client in quanto un utente potrebbe essere connesso a più di un dispositivo
                    .SendAsync("NewMessageReceived", message.ToDto());
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception) //quando un utente su disconnette dal signalR, automaticamente viene rimosso dal group
    {
        await messageRepository.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    //quando ci connettiamo all'hub vogliamo aggiungere l'utente al gruppo e persistere la sua connessione nel db
    private async Task<bool> AddToGroup(string groupName) 
    {
        var group= await messageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, GetUserId());

        if(group == null)
        {
            group= new Group(groupName);
            messageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);
        return await messageRepository.SaveAllAsync();
    }

    private static string GetGroupName(string? caller, string? other)
    {
        var stringCompare= string.CompareOrdinal(caller, other) <0; //ordinamento alfabetico
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        //in questo modo il groupName sarà sempre lo stesso indipendentemente dall'ordine con cui
        //i due si connettono
    }

    private string GetUserId()
    {
        return Context.User?.GetMemberId() ?? throw new HubException("Cannot get member id");
    }
}
