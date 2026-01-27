using System;
using API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace API.Data;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Member> Members { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<MemberLike> Likes {get; set;}
    public DbSet<Message> Messages {get; set;}
    public DbSet<Group> Groups { get; set; }
    public DbSet<Connection> Connections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>()
            .HasOne(x=>x.Recipient) //un messaggio ha un destinatario
            .WithMany(m=> m.MessagesReceived) //il destinatario può avere many messaggi ricevuti
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(x=>x.Sender) //un messaggio ha un mittente
            .WithMany(m=> m.MessagesSent) //il destinatario può avere many messaggi mandati
            .OnDelete(DeleteBehavior.Restrict);

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

        var nullabledateTimeConverter= new ValueConverter<DateTime?, DateTime?>( //vogliamo convertire un dateTime OPTIONAL (?) in un ALTRO DateTime OPTIONAL
            v=> v.HasValue ? v.Value.ToUniversalTime() : null,
            v=> v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties()) //per ogni proprietà/campo che ha come tipo dateTime lo convertiamo in utc
            {
                if(property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }else if(property.ClrType== typeof(DateTime?))
                {
                    property.SetValueConverter(nullabledateTimeConverter);
                }
            }
        }
    }
}

