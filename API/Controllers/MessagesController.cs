using System;
using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using Core.Entities;
using Infrastructure.Extensions;
using Core.Helpers;
using API.DTOs;


namespace API.Controllers;

public class MessagesController(IUnitOfWork uow) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var sender= await uow.MemberRepository.GetMemberByIdAsync(User.GetMemberId());  //User.GetMemberId() è un’altra extension method che legge l’ID dell’utente loggato dal JWT
        var recipient= await uow.MemberRepository.GetMemberByIdAsync(createMessageDto.RecipientId); //prendiamo il destinatario dal dto che passiamo quando richiamiamo l'api POST
        if(sender == null || recipient== null || sender.Id == createMessageDto.RecipientId)
        {
            return BadRequest("Cannot send this message");
        }
        var message= new Message
        {
            SenderId= sender.Id,
            RecipientId= recipient.Id,
            Content= createMessageDto.Content,
            Sender = sender,
            Recipient = recipient
        };
        uow.MessageRepository.AddMessage(message);
        MessageDto messageDto = new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderDisplayName = message.Sender.DisplayName,
            SenderImageUrl = message.Sender.ImageUrl,
            RecipientId = message.RecipientId,
            RecipientDisplayName = message.Recipient.DisplayName,
            RecipientImageUrl = message.Recipient.ImageUrl,
            Content = message.Content,
            DateRead = message.DateRead,
            MessageSent = message.MessageSent
        };
        if (await uow.Complete()) return messageDto;        //message.ToDto();

        return BadRequest("Failed to send message");
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessagesByContainer([FromQuery]MessageParams messageParams)
    {
        messageParams.MemberId= User.GetMemberId();
        var messages= await uow.MessageRepository.GetMessagesForMember(messageParams);
        var dtoItems = messages.Items.Select(m => new MessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderDisplayName = m.Sender?.DisplayName ?? string.Empty,
            SenderImageUrl = m.Sender?.ImageUrl,
            RecipientId = m.RecipientId,
            RecipientDisplayName = m.Recipient?.DisplayName ?? string.Empty,
            RecipientImageUrl = m.Recipient?.ImageUrl,
            Content = m.Content,
            DateRead = m.DateRead,
            MessageSent = m.MessageSent
        }).ToList();

        var result = new PaginatedResult<MessageDto>
        {
            Metadata = messages.Metadata,
            Items = dtoItems
        };
        return Ok(result); 
    }

    [HttpGet("thread/{recipientId}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessageThread(string recipientId)
    {
        var messagesThread = await uow.MessageRepository.GetMessageThread(User.GetMemberId(), recipientId);
        var dtoItems = messagesThread.Select(m => new MessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderDisplayName = m.Sender.DisplayName,
            SenderImageUrl = m.Sender.ImageUrl,
            RecipientId = m.RecipientId,
            RecipientDisplayName = m.Recipient.DisplayName,
            RecipientImageUrl = m.Recipient.ImageUrl,
            Content = m.Content,
            DateRead = m.DateRead,
            MessageSent = m.MessageSent
        });
        return Ok(dtoItems);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(string id)
    {
        var memberId= User.GetMemberId();
        var message= await uow.MessageRepository.GetMessage(id);
        if(message== null) return BadRequest("Cannot Delete this message");
        if(message.SenderId != memberId && message.RecipientId != memberId) return BadRequest("You cannot delete this message");
        if(message.SenderId == memberId) message.SenderDeleted = true; // ci rendiamo conto se a cancellare sia il mittente
        if(message.RecipientId == memberId) message.RecipientDeleted = true;

        if(message is {SenderDeleted: true, RecipientDeleted: true }) //funzione che matcha con i property parttern, vediamo se viene cancellato da entrambe le parti
        {
            uow.MessageRepository.DeleteMessage(message);
        }

        if(await uow.Complete()) return Ok();
        return BadRequest("Problem deleting message");
    }

}
