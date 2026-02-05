using System;

namespace Core.Entities;

public class Connection(string connectionId, string userId)
{
    public string ConnectionId { get; set; } = connectionId;
    public string UserId { get; set; }= userId;

    //nav prop per evitare che il groupname che è la nav prop di Group sia nullable qui dentro
    public Group Group { get; set; }= null!;
}
