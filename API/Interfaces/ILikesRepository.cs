using System;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesRepository
{
    Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId); //questo metodo ritorna un like specifico 
    Task<PaginatedResult<Member>> GetMemberLikes(LikesParams likesParams); //ritorna una lista di members che sono stati liked o che hanno messo like a un certo member o reciproco
    Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId); //ritorna una lista di stringhe con gli id dei membri a cui l'utente corrente ha messo like
    void DeleteLike(MemberLike like);
    void AddLike(MemberLike like);
    Task<bool> SaveAllChanges();

}
