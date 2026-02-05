using System;
using Core.Entities;

namespace Core.Interfaces;

public interface IPhotoRepository
{
    Task<IReadOnlyList<Photo>> GetUnapprovedPhotos();
    Task<Photo?> GetPhotoById(int photoId);
    void RemovePhoto(Photo photo);
}
