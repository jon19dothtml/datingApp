using System.Security.Claims;
using Core.DTOs;
using Core.Helpers;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class MembersController(IUnitOfWork uow, 
        IPhotoService photoService) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers(
            [FromQuery] MemberParams memberParams) //siccome dobbiamo passare questo parametro come query e non come object useremo [FromQuery]
        {
            memberParams.CurrentMemberId=User.GetMemberId(); //ci settiamo il member id
            return Ok(await uow.MemberRepository.GetMembersAsync(memberParams));
        }

        [HttpGet("{id}")]   //localhost:5001/api/members/bob-id
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            var member= await uow.MemberRepository.GetMemberByIdAsync(id);
            if(member == null)
            {
                return base.BadRequest("No user found");
            }
            return member;
        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
        {
            var isCurrentUser= User.GetMemberId() == id; //== return boolean value
            return Ok(await uow.MemberRepository.GetPhotosByMemberIdAsync(id, isCurrentUser));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            // var memberId= User.FindFirstValue(ClaimTypes.NameIdentifier); //prendiamo l'id dell'utente loggato dai claims. 
            // // I claims sono delle informazioni sull'utente che vengono memorizzate nel token
            // //usiamo NameIdentifier perchè è quello che usiamo per l'id nell'autenticazione
            // if(memberId == null) return BadRequest("No id found in token"); 

            var memberId= User.GetMemberId(); //usiamo l'estensione che abbiamo creato per prendere l'id dell'utente loggato
            //usiamo User perchè è un oggetto di tipo ClaimsPrincipal
            

            var member= await uow.MemberRepository.getMemberForUpdate(memberId);
            if(member == null) return BadRequest("Member not found");
            //aggiorniamo le proprietà dell'utente con i valori del DTO
            member.DisplayName= memberUpdateDto.DisplayName ?? member.DisplayName;
            member.Description= memberUpdateDto.Description ?? member.Description;
            member.City= memberUpdateDto.City ?? member.City;
            member.Country= memberUpdateDto.Country ?? member.Country;
            member.User.DisplayName= memberUpdateDto.DisplayName ?? member.User.DisplayName;

            uow.MemberRepository.Update(member); //segniamo l'entità come modificata
            if(await uow.Complete()) return NoContent(); //se il salvataggio va a buon fine, ritorniamo 204 no content
            return BadRequest("Failed to update member");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<Photo>> AddPhoto([FromForm] IFormFile file)
        {
            var member= await uow.MemberRepository.getMemberForUpdate(User.GetMemberId()); //ci recuperiamo il membro loggato

            if(member==null) return BadRequest("Cannot Update Member");

            var result= await photoService.UploadPhotoAsync(file); //carichiamo la foto usando il servizio di foto

            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo= new Photo{ //aggiorniamo le proprietà della foto con i valori ritornati da Cloudinary
                Url=result.SecureUrl.AbsoluteUri,
                PublicId= result.PublicId,
                MemberId= User.GetMemberId()
            };

            // if(member.ImageUrl == null)  //se non abbiamo ancora un'immagine principale 
            // {
            //     member.ImageUrl= photo.Url; //impostiamo questa come immagine principale
            //     member.User.ImageUrl= photo.Url; //aggiorniamo anche l'immagine dell'utente
            // }

            member.Photos.Add(photo); //aggiungiamo la foto alla collezione di foto del membro

            if(await uow.Complete()) return photo; //se il salvataggio va a buon fine, ritorniamo la foto
            return BadRequest("Problem adding Photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var member= await uow.MemberRepository.getMemberForUpdate(User.GetMemberId());
            if(member==null) return BadRequest("Cannot get the member from token");
            var photo= member.Photos.SingleOrDefault(x=> x.Id== photoId); //registriamo la foto che vogliamo prendere come principale
            if(member.ImageUrl==photo?.Url || photo== null) //controlliamo se la foto principale che l'utente ha già non è quella che vogliamo impostare noi
            {
                return BadRequest("This is already your main photo");
            }

            member.ImageUrl=photo.Url;
            member.User.ImageUrl= photo.Url;

            if(await uow.Complete()) return NoContent();

            return BadRequest("Problem settings main photo");
        }
    
        
    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var member= await uow.MemberRepository.getMemberForUpdate(User.GetMemberId()); //ci recuperiamo il membro loggato
        
        if(member==null) return BadRequest("Cannot get the member from token");
        
        var photo= member.Photos.SingleOrDefault(x => x.Id == photoId); //registriamo la foto che vogliamo prendere come principale

        if(photo == null || photo.Url == member.ImageUrl) //controlliamo se la foto esiste e se non è la foto principale
        {
            return BadRequest("Cannot delete this photo");
        }
        if(photo.PublicId != null) //se la foto ha un PublicId, significa che è stata caricata su Cloudinary
        {
            var result= await photoService.DeletePhotoAsync(photo.PublicId); //chiamiamo il servizio di foto per eliminarla da Cloudinary
            if(result.Error != null){ //se c'è un errore, ritorniamo un bad request con il messaggio di errore
                return BadRequest(result.Error.Message);
            }
        }

        member.Photos.Remove(photo); //rimuoviamo la foto dalla collezione di foto del membro

        if(await uow.Complete()) return Ok(); //

        return BadRequest("Problem deleting photo");
    }

    [HttpGet("cities")]
    public async Task<IReadOnlyList<string>> GetCities()
    { 
        return await uow.MemberRepository.GetCities();
    }

    [HttpGet("countries")]
    public async Task<IReadOnlyList<string>> GetCountries()
    {
        return await uow.MemberRepository.GetCountries();
    }

    }
}
