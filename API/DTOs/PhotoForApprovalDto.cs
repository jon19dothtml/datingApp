using Core.Entities;
using System;

namespace Core.DTOs;

public class PhotoForApprovalDto
{
    public required int Id { get; set; }
    public required string Url { get; set; }
    public required string UserId { get; set; }
    public bool IsApproved { get; set; }
}
