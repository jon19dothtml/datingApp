using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class CreateMessage
    {
        public required string RecipientId { get; set; }
        public required string Content { get; set; }
    }
}
