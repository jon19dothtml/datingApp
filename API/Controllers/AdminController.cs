using System;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Core.DTOs;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager, IUnitOfWork uow, IPhotoService photoService) : BaseApiController
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
    public async Task<ActionResult<IEnumerable<Photo>>> GetPhotosForModeration()
    {
        var photo = await uow.PhotoRepository.GetUnapprovedPhotos();
        var photoList = new List<PhotoForApprovalDto>();
        foreach(var p in photo){
            var photoDto = new PhotoForApprovalDto
            {
                Id = p.Id,
                Url = p.Url,
                UserId = p.MemberId,
                IsApproved = p.IsApproved
            };
            photoList.Add(photoDto);
        }
        return Ok(photoList);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        var photo= await uow.PhotoRepository.GetPhotoById(photoId);
        if(photo==null) return BadRequest("Could not get photo from db");
        photo.IsApproved=true;

        var member= await uow.MemberRepository.getMemberForUpdate(photo.MemberId); //ci recuperiamo il membro loggato
        if(member==null) return BadRequest("Could not get member");
        if(member.ImageUrl == null)  //se non abbiamo ancora un'immagine principale 
        {
            member.ImageUrl= photo.Url; //impostiamo questa come immagine principale
            member.User.ImageUrl= photo.Url; //aggiorniamo anche l'immagine dell'utente
        }

        await uow.Complete();
        return Ok();
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        var photo= await uow.PhotoRepository.GetPhotoById(photoId);
        if(photo==null) return BadRequest("Could not get photo from db");
        if(photo.PublicId != null) //controlliamo se è presente anche in cloudinary
        {
            var result= await photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Result == "ok") uow.PhotoRepository.RemovePhoto(photo);
        }
        else 
        { //altrimenti vorrà dire che è presente solo sul nostro db
            uow.PhotoRepository.RemovePhoto(photo);
        }
        await uow.Complete();
        return Ok();
    }
    
}
