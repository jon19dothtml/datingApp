using System;

using Core.Entities;

using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using Infrastructure.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.SignalR;

[Authorize]
public class MessageHub(IUnitOfWork uow, 
    IHubContext<PresenceHub> presenceHub, IMessageHubService messageHubService, IHubService hubService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext= Context.GetHttpContext(); //qui inizia la negoziazione principale
        var otherUser= httpContext?.Request?.Query["userId"].ToString()
             ?? throw new HubException("Other user not found");
        
        var groupName= GetGroupName(hubService.GetUserId(Context.User!), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await AddToGroup(groupName); //aggiungiamo al db il nome del group
        var messages= await uow.MessageRepository.GetMessageThread(hubService.GetUserId(Context.User!), otherUser);

        var hubMessages = new List<HubMessage>();
        foreach(var message in messages)
        {
            var hubMessage = messageHubService.CreateHubMessage(message);
            hubMessages.Add(hubMessage);
        }

        await Clients.Group(groupName).SendAsync("RecieveMessageThread", hubMessages);

    }

    public async Task SendMessage(CreateMessage createMessage)
    {
        var sender= await uow.MemberRepository.GetMemberByIdAsync(hubService.GetUserId(Context.User!));  //User.GetMemberId() è un’altra extension method che legge l’ID dell’utente loggato dal JWT
        var recipient= await uow.MemberRepository.GetMemberByIdAsync(createMessage.RecipientId); //prendiamo il destinatario dal dto che passiamo quando richiamiamo l'api POST
        if(sender == null || recipient== null || sender.Id == createMessage.RecipientId)
        {
            throw new HubException("Cannot send message");
        }
        var message= new Message
        {
            SenderId= sender.Id,
            RecipientId= recipient.Id,
            Content= createMessage.Content,
            Sender= sender,
            Recipient=recipient
        };

        var groupName= GetGroupName(sender.Id, recipient.Id);
        var group= await uow.MessageRepository.GetMessageGroup(groupName);
        var userInGroup= group != null && group.Connections.Any(x => x.UserId == message.RecipientId);

        if(userInGroup) //controlliamo se user e recipient sono in questo gruppo
        {
            message.DateRead = DateTime.UtcNow; 
        }
        uow.MessageRepository.AddMessage(message);
        if(await uow.Complete())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", messageHubService.CreateHubMessage(message));
            var connections= await PresenceTracker.GetConnectionsForUser(recipient.Id);
            if(connections != null && connections.Count >0 && !userInGroup)
            {
                await presenceHub.Clients.Clients(connections) //usiamo due volte Clients per specificare chi è il client in quanto un utente potrebbe essere connesso a più di un dispositivo
                    .SendAsync("NewMessageReceived", messageHubService.CreateHubMessage(message));
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception) //quando un utente su disconnette dal signalR, automaticamente viene rimosso dal group
    {
        await uow.MessageRepository.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    //quando ci connettiamo all'hub vogliamo aggiungere l'utente al gruppo e persistere la sua connessione nel db
    private async Task<bool> AddToGroup(string groupName) 
    {
        var group= await uow.MessageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, hubService.GetUserId(Context.User!));

        if(group == null)
        {
            group= new Group(groupName);
            uow.MessageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);
        return await uow.Complete();
    }

    private static string GetGroupName(string? caller, string? other)
    {
        var stringCompare= string.CompareOrdinal(caller, other) <0; //ordinamento alfabetico
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        //in questo modo il groupName sarà sempre lo stesso indipendentemente dall'ordine con cui
        //i due si connettono
    }

    
}


