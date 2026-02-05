using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class SeedUser
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string? ImageUrl { get; set; }
        public required string DisplayName { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public required string Gender { get; set; }
        public string? Description { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
    }
}
