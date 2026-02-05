using Core.Entities;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class MessageHubService : IMessageHubService
    {
        public HubMessage CreateHubMessage(Message message)
        {
            return new HubMessage
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
        }
    }
}
