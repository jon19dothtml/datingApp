using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities;

public class Member
{
    public string Id { get; set; }=null!;
    public DateOnly DateOfBirth { get; set; }
    public string? ImageUrl { get; set; }
    public required string DisplayName { get; set; }
    public DateTime Created { get; set; }= DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public required string Gender { get; set; }
    public string? Description { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }

    
    //NAVIGATION PROPERTY
    [JsonIgnore] //per non ritornali quando ritorniamo il member
    public List<Photo> Photos {get; set;} =[];


    [JsonIgnore] //per non ritornali quando ritorniamo il member
    public List<MemberLike> LikedByMembers { get; set; }= []; //ogni member ha due liste, una di like ricevuti e una di like dati
    [JsonIgnore]
    public List<MemberLike> LikedMembers { get; set; } =[];

    [JsonIgnore] //per non ritornali quando ritorniamo il member
    [ForeignKey(nameof(Id))]
    public AppUser User { get; set; } =null!;
    

}
