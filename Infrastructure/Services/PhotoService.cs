using System;
using Core.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Core.Helpers;

namespace Infrastructure.Services;

public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;
    public PhotoService(IOptions<CloudinarySettings> config) // Dependency Injection of Cloudinary settings
    { //il costruttore usa IOptions per ottenere le impostazioni di Cloudinary dal file di configurazione. 
    // IOptions è un pattern comune in ASP.NET Core per gestire le configurazioni fortemente tipizzate.
        var account = new Account(config.Value.CloudName, // Crea un oggetto Account utilizzando le impostazioni fornite
            config.Value.ApiKey, config.Value.ApiSecret);

        _cloudinary = new Cloudinary(account); // Inizializza l'istanza di Cloudinary con l'account creato
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var deleteParams= new DeletionParams(publicId);
        return await _cloudinary.DestroyAsync(deleteParams);
    }

    public async Task<ImageUploadResult> UploadPhotoAsync(IFormFile file)
    {
        var uploadedResult= new ImageUploadResult();
        if (file.Length > 0)
        { //file.OpenReadStream ci fornisce un flusso di dati e Cloudinary può prendere questo 
        //flusso come elemento da utilizzare per il caricamento
            await using var stream = file.OpenReadStream(); //usiamo using in modo che cio
            // che facciamo qui venga distrutto quando il nostro componente viene distrutto
            var uploadParams= new ImageUploadParams
            {
                File= new FileDescription(file.FileName, stream),
                Transformation= new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"), //trasformiamo l'immagine in un aspetto che vogliamo
                Folder="da-ang21"
            };

            uploadedResult= await _cloudinary.UploadAsync(uploadParams);
        }

        return uploadedResult;
    }
}
