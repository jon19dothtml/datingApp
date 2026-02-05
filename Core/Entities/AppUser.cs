using Microsoft.AspNetCore.Identity;

namespace Core.Entities;

public class AppUser: IdentityUser
{
    public required string DisplayName { get; set; }
    public string? ImageUrl { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    //Nav prop
    public Member Member { get; set; }=null!;
    
    
}
