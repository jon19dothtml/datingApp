using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces
{
    public interface IUserMangerService
    {
        Task<User> MapUser(AppUser user);
    }
}
