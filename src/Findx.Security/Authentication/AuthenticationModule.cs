﻿using Findx.AspNetCore;
using Findx.Extensions;
using Findx.Modularity;
using Findx.Security.Authentication.Cookie;
using Findx.Security.Authentication.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel;
using System.Text;
namespace Findx.Authorization.Authentication
{
    [Description("Findex-认证模块")]
    public class AuthenticationModule : AspNetCoreModuleBase
    {
        public override ModuleLevel Level => ModuleLevel.Application;
        public override int Order => 10;
        private bool Enabled;
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            IConfiguration configuration = services.GetConfiguration();
            Enabled = configuration.GetValue<bool>("Findx:Authentication:Enabled");
            if (Enabled)
            {
                services.AddScoped<FindxCookieAuthenticationEvents>();
                services.AddScoped<FindxJwtBearerEvents>();
                services.AddScoped<IJwtTokenBuilder, DefaultJwtTokenBuilder>();

                var defaultScheme = configuration.GetValue<string>("Findx:Authentication:DefaultScheme");

                AuthenticationBuilder builder = services.AddAuthentication(defaultScheme ?? JwtBearerDefaults.AuthenticationScheme);

                AddJwtBearer(services, builder);
                AddCookie(services, builder);
            }
            return services;
        }
        public override void UseModule(IApplicationBuilder app)
        {
            if (Enabled)
            {
                app.UseAuthentication();
                base.UseModule(app);
            }
        }


        protected virtual AuthenticationBuilder AddJwtBearer(IServiceCollection services, AuthenticationBuilder builder)
        {
            IConfiguration configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:Authentication:Jwt");
            JwtOptions jwt = section.Get<JwtOptions>();
            services.Configure<JwtOptions>(section);
            if (!jwt.Enabled)
            {
                return builder;
            }

            Check.NotNull(jwt.Secret, nameof(jwt.Secret));

            builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = jwt.Issuer ?? "findx",
                    ValidAudience = jwt.Audience ?? "findx",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    LifetimeValidator = (nbf, exp, token, param) => exp > DateTimeOffset.UtcNow
                };
                opts.Events = new FindxJwtBearerEvents();
            });

            return builder;
        }
        protected virtual AuthenticationBuilder AddCookie(IServiceCollection services, AuthenticationBuilder builder)
        {
            IConfiguration configuration = services.GetConfiguration();
            CookieOptions cookie = new();
            configuration.Bind("Findx:Authentication:Cookie", cookie);
            if (!cookie.Enabled)
            {
                return builder;
            }

            builder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opts =>
                {
                    if (cookie.CookieName != null)
                    {
                        opts.Cookie.Name = cookie.CookieName;
                    }

                    opts.LoginPath = cookie.LoginPath ?? opts.LoginPath;
                    opts.LogoutPath = cookie.LogoutPath ?? opts.LogoutPath;
                    opts.AccessDeniedPath = cookie.AccessDeniedPath ?? opts.AccessDeniedPath;
                    opts.ReturnUrlParameter = cookie.ReturnUrlParameter ?? opts.ReturnUrlParameter;
                    opts.SlidingExpiration = cookie.SlidingExpiration;

                    if (cookie.ExpireMins > 0)
                    {
                        opts.ExpireTimeSpan = TimeSpan.FromMinutes(cookie.ExpireMins);
                    }

                    opts.EventsType = typeof(FindxCookieAuthenticationEvents);
                });
            return builder;
        }
    }
}
