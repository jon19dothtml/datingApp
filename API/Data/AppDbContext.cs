using System;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<MemberLike> Likes {get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MemberLike>().
            HasKey(x=> new {x.SourceMemberId, x.TargetMemberId}); //configuriamo manualmente queste due property come PK, in quanto nell'entity 
                                                     //memberLike non c'è una prop Id e qui stiamo creando la tabella che sta tra due Many:Many
        
        //relazione one to many, one currentUser can like many user
        modelBuilder.Entity<MemberLike>()
            .HasOne(s=> s.SourceMember)
            .WithMany(t=> t.LikedMembers)
            .HasForeignKey(s=> s.SourceMemberId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MemberLike>()
            .HasOne(s=> s.TargetMember)
            .WithMany(t => t.LikedByMembers)
            .HasForeignKey(s=> s.TargetMemberId)
            .OnDelete(DeleteBehavior.NoAction);


        var dateTimeConverter= new ValueConverter<DateTime, DateTime>( //vogliamo convertire un dateTime in un altro DateTime
            v=> v.ToUniversalTime(),
            v=> DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties()) //per ogni proprietà/campo che ha come tipo dateTime lo convertiamo in utc
            {
                if(property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
            }
        }
    }
}
