using Core.Entities;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class UserManagerService(ITokenService tokenService) : IUserMangerService
    {
        public async Task<User> MapUser(AppUser user)
        {
            return new User
            {
                Id = user.Id,
                Email = user.Email!,
                DisplayName = user.DisplayName,
                ImageUrl = user.ImageUrl,
                Token = await tokenService.CreateToken(user)
            };
        }
    }
}
