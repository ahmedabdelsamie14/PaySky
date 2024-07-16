using Infrastructure.Data;
using Core.Interfaces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Infrastructure.Custom_Middlewares;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Core.Models;
using ApplicationLayer.Helper;
using Microsoft.Extensions.Logging;

namespace EmploymentSystemApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddScoped<IApplicantRepository, ApplicantRepository>();
            builder.Services.AddScoped<IEmployerRepository, EmployerRepository>();
            builder.Services.AddScoped<IVacancyRepository, VacancyRepository>();
            builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();

            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            builder.Services.AddMemoryCache();

            var key = Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JwtSettings:JwtKey").Value!);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = "Session";
                options.IdleTimeout = TimeSpan.FromMinutes(20); 
            });
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAuthorization(options => 
                    {
                        options.AddPolicy("EmployerOnly", policy => policy.RequireClaim(ClaimTypes.Role, "Employer"));
                        options.AddPolicy("ApplicantOnly", policy => policy.RequireClaim(ClaimTypes.Role, "Applicant"));
                    });
            

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<EmploymentSystemDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                        c.RoutePrefix = string.Empty;
                    });
                }
            }

            app.UseHttpsRedirection();

            app.UseSession();

            app.UseMiddleware<SessionTokenMiddleware>();

            app.UseAuthentication();
         
            app.UseMiddleware<UnAuthMiddleware>();

            app.UseAuthorization();


            

            app.MapControllers();
            app.Run();


        }
    }
}