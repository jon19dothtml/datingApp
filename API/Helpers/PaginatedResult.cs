using System;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers;

public class PaginatedResult<T>
{
    public PaginationMetadata Metadata { get; set; }= default!;
    public List<T> Items { get; set; }= [];
};

public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

}; 
public class PaginationHelper{
    public static async Task<PaginatedResult<T>> CreateAsync<T>(IQueryable<T> query, int pageNumber, int pageSize){
    var count = await query.CountAsync(); //troviamo la quantità di elementi disponibili nel db 
    var items= await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(); 
    //con skip saltiamo gli elementi delle pagine precedenti e con take prendiamo solo gli elementi della pagina corrente
    //quindi diciamo salta tutte le pagine meno quella corrente moltiplicato per la dimensione della pagina e prendi solo gli elementi della pagina corrente
    //in questo caso stiamo eseguendo due interrogazioni al db una per il conteggio e una per prendere gli elementi (toListAsync)
    return new PaginatedResult<T>
    {
        Metadata= new PaginationMetadata
        {
            CurrentPage = pageNumber,
            TotalPages= (int)Math.Ceiling(count/(double)pageSize),
            PageSize= pageSize,
            TotalCount= count
        },
        Items=items
    };
}
}
