using System;
using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class Group(string name)
{
    [Key] //rendiamo questa chiave come primaria, di conseguenza sarà anche indicizzata
    public string Name { get; set; }= name;
    
    //nav prop
    public ICollection<Connection> Connections { get; set; }= [];
}
