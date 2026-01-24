using System;
using API.Data;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController(ILikesRepository likesRepository) : BaseApiController
{
    [HttpPost("{targetMemberId}")]
    public async Task<ActionResult> ToggleLike(string targetMemberId) // mette o toglie il like in base al suo status (se non esiste, lo mette altrimenti lo toglie)
    {
        var sourceMemberId= User.GetMemberId();

        if(sourceMemberId==targetMemberId) return BadRequest("You can't like yourself");

        var existingLike= await likesRepository.GetMemberLike(sourceMemberId, targetMemberId);

        if (existingLike == null)
        {
            var like= new MemberLike
            {
                SourceMemberId= sourceMemberId,
                TargetMemberId=targetMemberId
            };
            likesRepository.AddLike(like);
        }
        else
        {
            likesRepository.DeleteLike(existingLike);
        }

        if (await likesRepository.SaveAllChanges()) return Ok();

        return BadRequest("Failed to upload like");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCurrentMemberLikeIds() //visualizziamo chi piace al currentUser
    {
        return Ok(await likesRepository.GetCurrentMemberLikeIds(User.GetMemberId())); 
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Member>>>GetMemberLikes([FromQuery] LikesParams likesParams)
    {
        likesParams.UserId= User.GetMemberId();
        var members= await likesRepository.GetMemberLikes(likesParams);
        return Ok(members);
    } //qui possiamo visualizzare a chi ho messo mi piace, chi l'ha messo a me e mutual
}
