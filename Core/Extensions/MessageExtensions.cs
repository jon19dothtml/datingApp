//using System;
//using System.Linq.Expressions;
//using Core.DTOs;
//using Core.Entities;

//namespace Core.Extensions;

//public static class MessageExtensions
//{
//    public static MessageDto ToDto(this Message message) 
//    { //con il this questo metodo è come se fosse un metodo dell’oggetto Message”.
//    //  Senza this, il metodo è solo un normale metodo statico:
//        return new MessageDto
//        {
//            Id= message.Id,
//            SenderId= message.SenderId,
//            SenderDisplayName= message.Sender.DisplayName,
//            SenderImageUrl= message.Sender.ImageUrl,
//            RecipientId= message.RecipientId,
//            RecipientDisplayName= message.Recipient.DisplayName,
//            RecipientImageUrl=message.Recipient.ImageUrl,
//            Content= message.Content,
//            DateRead= message.DateRead,
//            MessageSent= message.MessageSent
//        };
//    }

//    public static Expression<Func<Message, MessageDto>> ToDtoProjection() //un expression richiede una func con due parametri, 
//    // il primo è quello in entrata e il secondo è quello in uscita
//    {
//        return message=> new MessageDto
//        {
//            Id= message.Id,
//            SenderId= message.SenderId,
//            SenderDisplayName= message.Sender.DisplayName,
//            SenderImageUrl= message.Sender.ImageUrl,
//            RecipientId= message.RecipientId,
//            RecipientDisplayName= message.Recipient.DisplayName,
//            RecipientImageUrl=message.Recipient.ImageUrl,
//            Content= message.Content,
//            DateRead= message.DateRead,
//            MessageSent= message.MessageSent
//        };
//    }
//}
