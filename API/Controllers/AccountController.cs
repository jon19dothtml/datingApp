using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")] //api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        // if (await EmailExists(registerDto.Email))
        //     return BadRequest("Email is already in use");
//usiamo sempre await perche scarta il boolean del metodo EmailExists, quindi bisogna
//attendere il risultato prima di procedere
        // using var hmac= new HMACSHA512(); 
        //stiamo usando using perche HMACSHA512 implementa IDisposable, 
        // che è un'interfaccia che richiede la pulizia delle risorse non gestite
        var user= new AppUser
        {
            DisplayName= registerDto.DisplayName,
            Email= registerDto.Email,
            UserName = registerDto.Email,
            Member = new Member
            {
                DisplayName= registerDto.DisplayName,
                Gender= registerDto.Gender,
                City= registerDto.City,
                Country=registerDto.Country,
                DateOfBirth= registerDto.DateOfBirth
            }
        };

        var result= await userManager.CreateAsync(user, registerDto.Password); // crea sia hash che salt
        if (!result.Succeeded)
        {
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError("identity", error.Description);
            }

            return ValidationProblem();
        }
        await userManager.AddToRoleAsync(user, "Member"); //aggiungiamo un ruolo a chi si registra

        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService); //passiamo solo il tokenService come argomento,
        //perche l'oggetto user viene passato implicitamente come parametro this tramite il metodo di estensione
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user= await userManager.FindByEmailAsync(loginDto.Email);

        if(user==null) return Unauthorized("Invalid email");

        var result= await userManager.CheckPasswordAsync(user, loginDto.Password); //controllo della password

        if(!result) return Unauthorized("Invalid password");

        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserDto>> RefreshToken()
    {
        var refreshToken= Request.Cookies["refreshToken"];
        if(refreshToken==null) return NoContent();

        var user= await userManager.Users.FirstOrDefaultAsync(x=> x.RefreshToken==refreshToken 
            && x.RefreshTokenExpiry>DateTime.UtcNow);

        if(user==null) return Unauthorized();
        await SetRefreshTokenCookie(user);
        return await user.ToDto(tokenService);
    }

    private async Task SetRefreshTokenCookie(AppUser user)
    {
        var refreshToken= tokenService.GenerateRefreshToken();
        user.RefreshToken= refreshToken;
        user.RefreshTokenExpiry= DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user); //aggiorniamo il db

        var cookieOptions= new CookieOptions
        {
            HttpOnly=true, //questo cookie non è accessibile per il nostro client ma solo per il backend
            Secure=true,
            SameSite=SameSiteMode.Strict,
            Expires= DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}




        //la password salt è un byte array che viene usato per creare l'hash della password
        // using var hmac= new HMACSHA512(user.PasswordSalt); //ricreiamo l'istanza di HMACSHA512 con la password salt salvata nel db
        // var computedHash= hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password)); //calcoliamo l'hash della password inviata dall'utente
        // for (var i=0; i< computedHash.Length; i++) //siccome si tratta di byte array, dobbiamo confrontarli elemento per elemento
        // {
        //     if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
        // }
