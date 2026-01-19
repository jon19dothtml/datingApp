using System;
using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Extensions;

public static class AppUserExtensions //quando rendiamo static una classe significa che non possiamo istanziarla ma possiamo solo usare i suoi metodi statici
{
    public static UserDto ToDto(this AppUser user, ITokenService tokenService)
    {
        return new UserDto
        {
            Id= user.Id,
            Email= user.Email,
            DisplayName= user.DisplayName,
            ImageUrl= user.ImageUrl,
            Token= tokenService.CreateToken(user)
        };
    }
}
