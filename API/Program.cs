using API.Data;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Middleware;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container. Se vogliamo che siano iniettabili, dobbiamo dichiararli come Services

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    opt.UseAsyncSeeding(async (context, _, cancellationToken) =>
{
    var adminRole = await context.Set<IdentityRole>().FirstOrDefaultAsync(r => r.Name == "Admin");
    if (adminRole == null)
    {
        await context.Set<IdentityRole>().AddAsync(new IdentityRole
        {
            Id = "admin-id",
            Name = "Admin",
            NormalizedName = "ADMIN"
        },
        cancellationToken
        );
    }
    var memberRole = await context.Set<IdentityRole>().FirstOrDefaultAsync(r => r.Name == "Member");
    if (memberRole == null)
    {
        await context.Set<IdentityRole>().AddAsync(new IdentityRole
        {
            Id = "member-id",
            Name = "Member",
            NormalizedName = "MEMBER"
        },
        cancellationToken
        );
    }
    var moderatorRole = await context.Set<IdentityRole>().FirstOrDefaultAsync(r => r.Name == "Moderator");
    if (moderatorRole == null)
    {
        await context.Set<IdentityRole>().AddAsync(new IdentityRole
        {
            Id = "moderator-id",
            Name = "Moderator",
            NormalizedName = "MODERATOR"
        },
        cancellationToken
        );
    }
    await context.SaveChangesAsync(cancellationToken);
});
opt.UseSeeding((context, _) =>
{
    var adminRole = context.Set<IdentityRole>().FirstOrDefault(r => r.Name == "Admin");
    if (adminRole == null)
    {
        context.Set<IdentityRole>().Add(new IdentityRole
        {
            Id = "admin-id",
            Name = "Admin",
            NormalizedName = "ADMIN"
        }
        );        
    }
    var memberRole = context.Set<IdentityRole>().FirstOrDefault(r => r.Name == "Member");
    if (memberRole == null)
    {
        context.Set<IdentityRole>().Add(new IdentityRole
        {
            Id = "member-id",
            Name = "Member",
            NormalizedName = "MEMBER"
        }
        );
    }
    var moderatorRole = context.Set<IdentityRole>().FirstOrDefault(r => r.Name == "Moderator");
    if (moderatorRole == null)
    {
        context.Set<IdentityRole>().Add(new IdentityRole
        {
            Id = "moderator-id",
            Name = "Moderator",
            NormalizedName = "MODERATOR"
        }
        );
    }
    context.SaveChanges();
});
});

builder.Services.AddCors();
builder.Services.AddScoped<ITokenService, TokenService>();
    //quando registriamo un servizio abbiamo tre opzioni: transient, scoped e singleton
    //Scoped significa che il servizio viene creato una volta per richiesta http
    //transient significa che viene creato ogni volta che viene richiesto
    //singleton significa che viene creato una volta sola per tutta l'applicazione
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ILikesRepository, LikesRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<LogUserActivity>(); //aggiungiamo il nostro action filter come servizio iniettabile
builder.Services.Configure<CloudinarySettings>(builder.Configuration
    .GetSection("CloudinarySettings")); // configuriamo le impostazioni di Cloudinary

builder.Services.AddIdentityCore<AppUser>(opt=>
{
    opt.Password.RequireNonAlphanumeric=false;
    opt.User.RequireUniqueEmail=true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenKey = builder.Configuration["TokenKey"] 
            ?? throw new Exception("TokenKey not found -Program.cs");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequiredAdminRole", policy => policy.RequireRole("Admin"))
    .AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
var app = builder.Build();


//middleware e services sono due cose diverse, 
//i middleware sono componenti che elaborano le richieste HTTP 
//in entrata e possono modificare la risposta HTTP in uscita
//i servizi sono classi che forniscono funzionalità specifiche all'interno dell'applicazione
// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    .WithOrigins("http://localhost:4200", "https://localhost:4200"));
//il cors è un middleware che permette di configurare le richieste
// cross-origin cioè provenienti da domini diversi

app.UseAuthentication(); //prima autenticazione poi autorizzazione
app.UseAuthorization();
app.MapControllers();

//andiamo ad automatizzare migration e se non ci sono utenti andiamo a fare seeding
using var scope= app.Services.CreateScope(); //createScope serve per creare un ambito per i servizi con scoped lifetime
var services= scope.ServiceProvider; //get the service provider from the scope
try
{
    var context= services.GetRequiredService<AppDbContext>(); //get the dbcontext from the service provider
    var userManager= services.GetRequiredService<UserManager<AppUser>>();
    await context.Database.MigrateAsync(); //apply any pending migrations
    await Seed.SeedUser(userManager);
}
catch (Exception ex)
{
    var logger= services.GetRequiredService<ILogger<Program>>(); //get the logger from the service provider
    logger.LogError(ex, "an error occured during migration");
}

app.Run();
