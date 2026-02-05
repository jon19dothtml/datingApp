using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class User
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string DisplayName { get; set; }
        public string? ImageUrl { get; set; }
        public required string Token { get; set; }
    }
}
