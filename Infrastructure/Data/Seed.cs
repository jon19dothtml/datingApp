using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class Seed
{
    public sealed record LocalSeedUserDto()
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

    public static async Task SeedUser(UserManager<AppUser> userManager)
    {
        if (await userManager.Users.AnyAsync()) return;
        var memberData= await File.ReadAllTextAsync("UserSeedData.json");
        var members= JsonSerializer.Deserialize<List<LocalSeedUserDto>>(memberData);
        if (members == null)
        {
            Console.WriteLine("No menbers in seed data");
            return;
        }

        

        foreach(var member in members)
        {
            //using var hmac= new HMACSHA512(); //ogni utente avrà una sua chiave per l'hash della password, perche ad ogni iterazione viene instanziato un nuovo HMACSHA512

            var user= new AppUser
            {
                Id=member.Id,
                Email=member.Email,
                UserName= member.Email,
                DisplayName= member.DisplayName,
                ImageUrl= member.ImageUrl,
                Member = new Member
                {
                    Id= member.Id,
                    DisplayName=member.DisplayName,
                    Description=member.Description,
                    DateOfBirth=member.DateOfBirth,
                    ImageUrl=member.ImageUrl,
                    Gender= member.Gender,
                    City=member.City,
                    Country=member.Country,
                    LastActive=member.LastActive,
                    Created=member.Created
                }
            };

            user.Member.Photos.Add(new Photo
            {
                Url= member.ImageUrl!,
                MemberId= member.Id,
                IsApproved=true
            });

            var result= await userManager.CreateAsync(user, "Pa$$w0rd");
            if (!result.Succeeded)
            {
                Console.WriteLine(result.Errors.First().Description);
            }
            await userManager.AddToRoleAsync(user, "Member");
        }

        var admin= new AppUser
        {
            UserName="admin@test.com",
            Email= "admin@test.com",
            DisplayName="Admin"
        };

        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);
    }
}
