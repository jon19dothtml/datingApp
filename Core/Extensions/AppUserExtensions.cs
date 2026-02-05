//using System;

//using Core.Interfaces;
//using Core.Entities;

//namespace Core.Extensions;

//public static class AppUserExtensions //quando rendiamo static una classe significa che non possiamo istanziarla ma possiamo solo usare i suoi metodi statici
//{
//    public static async Task<UserDto> ToDto(this AppUser user, ITokenService tokenService)
//    {
//        return new UserDto
//        {
//            Id= user.Id,
//            Email= user.Email!,
//            DisplayName= user.DisplayName,
//            ImageUrl= user.ImageUrl,
//            Token= await tokenService.CreateToken(user)
//        };
//    }
//}
