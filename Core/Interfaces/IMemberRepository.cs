using Core.Entities;
using Core.Helpers;

namespace Core.Interfaces;

public interface IMemberRepository
{
    void Update(Member member);
    Task<PaginatedResult<Member>> GetMembersAsync(MemberParams memberParams);
    Task<Member?> GetMemberByIdAsync(string id);
    Task<IReadOnlyList<Photo>> GetPhotosByMemberIdAsync(string memberId, bool isCurrentUser);
    Task<Member?> getMemberForUpdate(string id);
    Task<IReadOnlyList<string>> GetCities();
    Task<IReadOnlyList<string>> GetCountries();

}
