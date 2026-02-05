using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Core.Interfaces;

namespace Infrastructure.Services;

public class TokenService(IConfiguration config, UserManager<AppUser> userManager) : ITokenService
{
    public async Task<string> CreateToken(AppUser user)
    {
        //configuriamo la chiave di sicurezza dalla configurazione dell appsettings.json, 
        // otterremo la nostra token key da li, iniettiamo la config tramite IConfiguration
        var tokenKey = config["TokenKey"] ?? throw new Exception("Token key not found in configuration");
        if(tokenKey.Length<64) throw new Exception("Token key must be at least 64 characters long"); //la chiave deve essere almeno di 64 caratteri per usare HmacSha512
        var key= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)); //creiamo la chiave di sicurezza simmetrica, la useremo sia per creare un token che per convalidarlo sul server
        
        var claims= new List<Claim> //creiamo i claims che vogliamo includere nel token
        {
            new Claim(ClaimTypes.Email, user.Email!),
            new (ClaimTypes.NameIdentifier, user.Id)
        };
        
        var roles= await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        var creds= new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature); //creiamo le credenziali di firma usando la chiave e l'algoritmo di firma HmacSha512
        var tokenDescriptor = new SecurityTokenDescriptor //descriviamo il token
        {
            Subject= new ClaimsIdentity(claims),
            Expires= DateTime.UtcNow.AddDays(15),
            SigningCredentials= creds
        };

        var tokenHandler= new JwtSecurityTokenHandler(); 
        //creiamo il gestore del token che implementa la classe che convalida e crea i token JWT
        var token= tokenHandler.CreateToken(tokenDescriptor); //richiamiamo il metodo per creare il token

        return tokenHandler.WriteToken(token); //ritorniamo il token serializzato come stringa
    }

    public string GenerateRefreshToken()
    {
        var randomBytes= RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes); //token a lunga durata che ritornerà al client con un cookie
    }
}
