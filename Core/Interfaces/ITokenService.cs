using System;
using Core.Entities;

namespace Core.Interfaces;

public interface ITokenService
{
    Task<string> CreateToken(AppUser user);
    string GenerateRefreshToken();
    
}
