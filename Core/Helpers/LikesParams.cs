using System;
using Core.Entities;

namespace Core.Helpers;

public class LikesParams : PagingParams
{
    public string UserId { get; set; } ="";
    // public required Member TargetMember{get; set; }
    public string Predicate { get; set; }="liked";
}
