using System;
using Core.Entities;
using Core.Helpers;
using Core.Interfaces;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class MemberRepository(AppDbContext context) : IMemberRepository
{
    public async Task<Member?> GetMemberByIdAsync(string id)
    {
        return await context.Members.FindAsync(id);
    }

    public async Task<Member?> getMemberForUpdate(string id)
    {
        return await context.Members
        .Include(x => x.User)
        .Include(x => x.Photos)
        .IgnoreQueryFilters()
        .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<PaginatedResult<Member>> GetMembersAsync(MemberParams memberParams)
    {

        var query = context.Members.AsQueryable(); // qui creiamo una query di base per prendere tutti i membri

        query = query.Where(x => x.Id != memberParams.CurrentMemberId); //ritorna tutti i param

        if (memberParams.Gender != null)
        {
            query = query.Where(x => x.Gender == memberParams.Gender);
        }

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MaxAge - 1)); //calcoliamo la data di nascita minima
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MinAge)); //calcoliamo la data di nascita massima

        query = query.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

        query = memberParams.OrderBy switch
        {
            "created" => query.OrderByDescending(x => x.Created),
            _ => query.OrderByDescending(x => x.LastActive) // qui per default abbiamo settato che ordini per LastActive
        };

        if (memberParams.City != null)
        {
            query = query.Where(c => c.City == memberParams.City);
        }

        if (!string.IsNullOrWhiteSpace(memberParams.Country))
        {
            query = query.Where(c =>
                c.Country.ToLower().Trim() == memberParams.Country.ToLower().Trim());
        }
        return await PaginationHelper.CreateAsync(query,
            memberParams.PageNumber, memberParams.PageSize);
    }

    public async Task<IReadOnlyList<Photo>> GetPhotosByMemberIdAsync(string memberId, bool isCurrentUser)
    {
        var query = context.Members
            .Where(x => x.Id == memberId)
            .SelectMany(x => x.Photos);

        if (isCurrentUser)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.ToListAsync();
    }


    public void Update(Member member)
    {
        context.Entry(member).State = EntityState.Modified; //qui diciamo che qualcosa di questa entità è stata modificata
    }

    public async Task<IReadOnlyList<string>> GetCities()
    {
        return await context.Members
            .Select(c => c.City)
            .Where(c => c != null && c != "")
            .Distinct()
            .ToListAsync();
    }
    public async Task<IReadOnlyList<string>> GetCountries()
    {
        return await context.Members
            .Select(c => c.Country)
            .Where(c => c != null && c != "")
            .Distinct()
            .ToListAsync();
    }
}
