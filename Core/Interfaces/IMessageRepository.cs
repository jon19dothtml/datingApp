using System;

using Core.Entities;
using Core.Helpers;

namespace Core.Interfaces;

public interface IMessageRepository
{
    void AddMessage(Message message);
    void DeleteMessage(Message message);
    Task<Message?> GetMessage(string messageId);
    Task<PaginatedResult<Message>> GetMessagesForMember(MessageParams messageParams);
    Task<IReadOnlyList<Message>> GetMessageThread(string currentMemberId, string recipientId);

    void AddGroup(Group group);
    Task RemoveConnection(string connectionId); // rimuoviamo la riga dal db
    Task<Connection?> GetConnection(string connectionId);
    Task<Group?> GetMessageGroup(string groupName);
    Task<Group?> GetGroupForConnection(string connectionId);
}
