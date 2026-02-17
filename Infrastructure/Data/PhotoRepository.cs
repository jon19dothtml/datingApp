using System;

using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class PhotoRepository(AppDbContext context) : IPhotoRepository
{

    public async Task<Photo?> GetPhotoById(int photoId) //no AsNoTracking perchè modifico una sua prop nell'admin controller
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x=> x.Id==photoId);
    }

    public async Task<IReadOnlyList<Photo>> GetUnapprovedPhotos()
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x=> x.IsApproved==false)
            .ToListAsync();
    }

    public void RemovePhoto(Photo photo)
    {
        context.Photos.Remove(photo);
    }
}
