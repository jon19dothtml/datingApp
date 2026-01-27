using System;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{

    private IMemberRepository? _memberRepository;
    private IMessageRepository? _messageRepository;
    private ILikesRepository? _likesRepository;
    public IMemberRepository MemberRepository => _memberRepository 
        ??= new MemberRepository(context); //ha il compito di inizializzare qui le repo 

    public IMessageRepository MessageRepository => _messageRepository 
        ??= new MessageRepository(context);

    public ILikesRepository LikesRepository => _likesRepository 
        ??= new LikesRepository(context);

    public async Task<bool> Complete() //Concetto di U.O.W. o tutte le modifiche funzionano o tutte le funzioni ricevano un ROLLBACK
    {
        try
        {
            return await context.SaveChangesAsync() >0;
        }catch (DbUpdateException ex)
        {
            throw new Exception("An error occured while saving changes", ex);
        }
    }

    public bool HasChanges()
    {
        return context.ChangeTracker.HasChanges();
    }
}
