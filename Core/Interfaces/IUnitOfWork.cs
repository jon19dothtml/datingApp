using System;

namespace Core.Interfaces;

public interface IUnitOfWork
{
    IMemberRepository MemberRepository {get;}
    IMessageRepository MessageRepository {get;}
    ILikesRepository LikesRepository {get;}
    IPhotoRepository PhotoRepository {get;}
    Task<bool> Complete(); // metodo che implementerà il saveChanges per tutti 
    bool HasChanges(); //tracker per vedere se ci sono delle modifiche al db prima di salvare qualcosa
}
