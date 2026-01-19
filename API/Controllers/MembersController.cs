using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MembersController(IMemberRepository memberRepository) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers()
        {
            return Ok( await memberRepository.GetMembersAsync());
        }

        [HttpGet("{id}")]   //localhost:5001/api/members/bob-id
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            var member= await memberRepository.GetMemberByIdAsync(id);
            if(member == null)
            {
                return base.BadRequest("No user found");
            }
            return member;
        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
        {
            return Ok(await memberRepository.GetPhotosByMemberIdAsync(id));
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
            

            var member= await memberRepository.getMemberForUpdate(memberId);
            if(member == null) return BadRequest("Member not found");
            //aggiorniamo le proprietà dell'utente con i valori del DTO
            member.DisplayName= memberUpdateDto.DisplayName ?? member.DisplayName;
            member.Description= memberUpdateDto.Description ?? member.Description;
            member.City= memberUpdateDto.City ?? member.City;
            member.Country= memberUpdateDto.Country ?? member.Country;
            member.user.DisplayName= memberUpdateDto.DisplayName ?? member.user.DisplayName;

            memberRepository.Update(member); //segniamo l'entità come modificata
            if(await memberRepository.SaveAllAsync()) return NoContent(); //se il salvataggio va a buon fine, ritorniamo 204 no content
            return BadRequest("Failed to update member");
        }
    }
}
