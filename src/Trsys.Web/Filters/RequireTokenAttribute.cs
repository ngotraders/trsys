﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Trsys.Web.Models.SecretKeys;
using Trsys.Web.Services;

namespace Trsys.Web.Filters
{
    public static class SecretKeyClaimsPrincipalFactory
    {
        private static readonly Tuple<string, SecretKeyType>[] secretKeyTypes = Enum.GetValues(typeof(SecretKeyType))
            .OfType<SecretKeyType>()
            .Select(key => Tuple.Create(Enum.GetName(typeof(SecretKeyType), key), key))
            .ToArray();

        public static ClaimsPrincipal Create(string key, SecretKeyType keyType)
        {
            var principal = new ClaimsPrincipal();
            var claims = new List<Claim>() { new Claim(ClaimsIdentity.DefaultNameClaimType, key) };
            foreach (var elem in secretKeyTypes)
            {
                if (keyType.HasFlag(elem.Item2))
                {
                    claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, elem.Item1));
                }
            }
            principal.AddIdentity(new ClaimsIdentity(claims, "SecretToken", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType));
            return principal;
        }
    }

    public class RequireTokenAttribute : ActionFilterAttribute
    {
        public SecretKeyType? KeyType { get; }

        public RequireTokenAttribute(string keyType = null)
        {
            if (!string.IsNullOrEmpty(keyType))
            {
                if (keyType == "Publisher")
                {
                    KeyType = SecretKeyType.Publisher;
                }
                else if (keyType == "Subscriber")
                {
                    KeyType = SecretKeyType.Subscriber;
                }
            }
        }


        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var token = context.HttpContext.Request.Headers["X-Secret-Token"];
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedObjectResult("X-Secret-Token not se.");
                return;
            }
            var service = (SecretKeyService)context.HttpContext.RequestServices.GetService(typeof(SecretKeyService));
            var result = await service.VerifyAndTouchSecretTokenAsync(token, KeyType) as SecretTokenVerifyResult;
            if (result == null)
            {
                context.Result = new UnauthorizedObjectResult("X-Secret-Token is invalid.");
                return;
            }
            context.HttpContext.User = SecretKeyClaimsPrincipalFactory.Create(result.SecretKey, result.KeyType);
            await base.OnActionExecutionAsync(context, next);
        }
    }
}