using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(AppDbContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")] //api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await EmailExists(registerDto.Email))
            return BadRequest("Email is already in use");
//usiamo sempre await perche scarta il boolean del metodo EmailExists, quindi bisogna
//attendere il risultato prima di procedere
        using var hmac= new HMACSHA512(); 
        //stiamo usando using perche HMACSHA512 implementa IDisposable, 
        // che è un'interfaccia che richiede la pulizia delle risorse non gestite
        var user= new AppUser
        {
            DisplayName= registerDto.DisplayName,
            Email= registerDto.Email,
            PasswordHash= hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt= hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.ToDto(tokenService); //passiamo solo il tokenService come argomento,
        //perche l'oggetto user viene passato implicitamente come parametro this tramite il metodo di estensione
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user= await context.Users.SingleOrDefaultAsync(x=> x.Email== loginDto.Email);

        if(user==null) return Unauthorized("Invalid email");

        using var hmac= new HMACSHA512(user.PasswordSalt); //ricreiamo l'istanza di HMACSHA512 con la password salt salvata nel db
        var computedHash= hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password)); //calcoliamo l'hash della password inviata dall'utente
        for (var i=0; i< computedHash.Length; i++) //siccome si tratta di byte array, dobbiamo confrontarli elemento per elemento
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
        }
        return user.ToDto(tokenService);
    }

    private async Task<bool> EmailExists(string email)
    {
        return await context.Users.AnyAsync(x=> x.Email.ToLower() == email.ToLower());
    }
}
