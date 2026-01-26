using System;
using API.DTOs;
using API.Interfaces;
using API.Helpers;
using Microsoft.AspNetCore.Mvc;

using API.Entities;
using API.Extensions;

namespace API.Controllers;

public class MessagesController(IMessageRepository messageRepository, 
    IMemberRepository memberRepository) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var sender= await memberRepository.GetMemberByIdAsync(User.GetMemberId());  //User.GetMemberId() è un’altra extension method che legge l’ID dell’utente loggato dal JWT
        var recipient= await memberRepository.GetMemberByIdAsync(createMessageDto.RecipientId); //prendiamo il destinatario dal dto che passiamo quando richiamiamo l'api POST
        if(sender == null || recipient== null || sender.Id == createMessageDto.RecipientId)
        {
            return BadRequest("Cannot send this message");
        }
        var message= new Message
        {
            SenderId= sender.Id,
            RecipientId= recipient.Id,
            Content= createMessageDto.Content,
        };
        messageRepository.AddMessage(message);
        if(await messageRepository.SaveAllAsync()) return message.ToDto();

        return BadRequest("Failed to send message");
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessagesByContainer([FromQuery]MessageParams messageParams)
    {
        messageParams.MemberId= User.GetMemberId();
        return await messageRepository.GetMessagesForMember(messageParams);
    }

    [HttpGet("thread/{recipientId}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessageThread(string recipientId)
    {
        return Ok(await messageRepository.GetMessageThread(User.GetMemberId(), recipientId));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(string id)
    {
        var memberId= User.GetMemberId();
        var message= await messageRepository.GetMessage(id);
        if(message== null) return BadRequest("Cannot Delete this message");
        if(message.SenderId != memberId && message.RecipientId != memberId) return BadRequest("You cannot delete this message");
        if(message.SenderId == memberId) message.SenderDeleted = true; // ci rendiamo conto se a cancellare sia il mittente
        if(message.RecipientId == memberId) message.RecipientDeleted = true;

        if(message is {SenderDeleted: true, RecipientDeleted: true }) //funzione che matcha con i property parttern, vediamo se viene cancellato da entrambe le parti
        {
            messageRepository.DeleteMessage(message);
        }

        if(await messageRepository.SaveAllAsync()) return Ok();
        return BadRequest("Problem deleting message");
    }

}
