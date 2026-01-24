using System;
using API.Entities;

namespace API.Helpers;

public class LikesParams : PagingParams
{
    public string UserId { get; set; } ="";
    // public required Member TargetMember{get; set; }
    public string Predicate { get; set; }="liked";
}
