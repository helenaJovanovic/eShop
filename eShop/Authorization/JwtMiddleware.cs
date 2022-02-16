using eShop.Helpers;
using eShop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShop.Authorization
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context, IUserService userService, IJwtUtils jwtUtils)
        {
            //Token uzima iz headera
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            //vrsi se validacija
            var userId = jwtUtils.ValidateJwtToken(token);
            if(userId != null)
            {
                //ako postoji takav korsnik dodaje se u context items da bi 
                //moglo da se pristupi User objektu svuda u okviru tog trenutnog requesta
                context.Items["User"] = userService.GetById(userId.Value);
            }

            await _next(context);
        }
        
    }
}
