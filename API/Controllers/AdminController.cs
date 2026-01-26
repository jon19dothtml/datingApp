using System;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager) : BaseApiController
{
    [Authorize(Policy = "RequiredAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users= await userManager.Users.ToListAsync();
        var userList= new List<object>();
        foreach(var user in users)
        {
            var roles= await userManager.GetRolesAsync(user);
            userList.Add(new //anonymus object
            {
                user.Id,
                user.Email,
                Roles= roles.ToList()
            });
        }
        return Ok(userList);
    }

    [Authorize(Policy="RequiredAdminRole")]
    [HttpPost("edit-roles/{userId}")]
    public async Task<ActionResult<IList<string>>> EditRoles(string userId, [FromQuery] string roles)
    {
        if(string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

        var selectedRoles= roles.Split(",").ToArray(); //prende i ruoli selezionati da quell'utente

        var user= await userManager.FindByIdAsync(userId); //recupera l'utente

        if(user==null) return BadRequest ("Could not retrieve user");

        var userRoles = await userManager.GetRolesAsync(user); //prende i ruoli già usati dall'utente
        var result= await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles)); //aggiunge i ruoli selezionati ad eccezione di quelli già presenti
        if(!result.Succeeded) return BadRequest("Failed to add roles");
        //se il ruolo è già selezionato lo inviamo
        result= await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if(!result.Succeeded) return BadRequest("Failed to remove from roles");
        return Ok(await userManager.GetRolesAsync(user));
    }



    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public ActionResult GetPhotosForModeration()
    {
        return Ok("Admins or moderators can see this");
    }
    
}
