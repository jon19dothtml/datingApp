using System;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository(AppDbContext context) : ILikesRepository
{
    public void AddLike(MemberLike like)
    {
        context.Add(like);
    }

    public void DeleteLike(MemberLike like)
    {
        context.Remove(like);
    }

    public async Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string? memberId) //quando ritorna una lista si usa nel return un ToListAsync 
    {
        return await context.Likes
            .Where(x=> x.SourceMemberId == memberId) //controlliamo se il currentUser è uguale all'id che stiamo passando
            .Select(x=> x.TargetMemberId) //se è vero selezioniamo tutti i TargetMemberId, cioè tutti gli id dei membri a cui l'utente corrente ha messo like
            .ToListAsync();
    }

    public async Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId)
    {
        return await context.Likes.FindAsync(sourceMemberId, targetMemberId);
    }

    public async Task<PaginatedResult<Member>> GetMemberLikes(LikesParams likesParams) 
    {
        var query= context.Likes.AsQueryable(); //creiamo una query di base sulla tabella Likes
        IQueryable<Member> result;
        switch (likesParams.Predicate)
        {
            case "liked": //QUI VOGLIAMO I MEMBRI CHE L'UTENTE CORRENTE HA LIKATO
                result= query
                    .Where(like=> like.SourceMemberId==likesParams.UserId)
                    .Select(like=> like.TargetMember); //ritorna Member quindi selezioniamo la prop di tipo member
                break;
            case "likedBy" : //QUI VOGLIAMO I MEMBRI CHE HANNO LIKATO L'UTENTE CORRENTE
                result= query
                    .Where(like=> like.TargetMemberId==likesParams.UserId)
                    .Select(like=> like.SourceMember);
                break;
            default: //mutal
                var likeIds= await GetCurrentMemberLikeIds(likesParams.UserId); //qui prendo la lista di tutti gli utenti a cui il currentUser ha messo like
                result = query 
                    .Where(like=> like.TargetMemberId==likesParams.UserId 
                    && likeIds.Contains(like.SourceMemberId))
                    .Select(like=> like.SourceMember);
                break;
        }
        return await PaginationHelper.CreateAsync(result, 
            likesParams.PageNumber, likesParams.PageSize);
    }


}
