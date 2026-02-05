using System;
using Infrastructure.Data;

using Infrastructure.Data;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        if(context.HttpContext.User.Identity?.IsAuthenticated != true) return; //controlla se l'utente sia autenticato

        var memberId= resultContext.HttpContext.User.GetMemberId();

        var dbContext= resultContext.HttpContext.RequestServices.
            GetRequiredService<AppDbContext>();

        await dbContext.Members.Where(x=> x.Id == memberId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x=> x.LastActive, DateTime.UtcNow)); 
            //questo metodo aggiorna direttamente nel db senza dover prima recuperare l'entità

    }
}
