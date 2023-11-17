using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using ApiReview.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace ApiReview;

public class Startup
{
    public Startup(IConfiguration configuration)
    {JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler
            = ReferenceHandler
                .IgnoreCycles); // para solucionar el error de entra en bucle el sql porque hay una relacion de muchos a muchos

        //Add DbContext
        services.AddDbContext<AplicationDbContext>(options =>
        {
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddAutoMapper(typeof(Startup));

        //Add Identity
        services.AddIdentity<IdentityUser, IdentityRole>(options => { options.SignIn.RequireConfirmedAccount = false; })
            .AddEntityFrameworkStores<AplicationDbContext>().AddDefaultTokenProviders();

        //Add Authentication Jwt
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; //para que por defecto use jwt
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true; //para que guarde el token
            options.RequireHttpsMetadata = false; //para que no use https

            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true, //valida el emisor
                ValidateAudience = true, //valida el receptor
                //ValidateLifetime = true,//valida el tiempo de vida
                //ValidateIssuerSigningKey = true,//valida la firma
                ValidIssuer = Configuration["JWT:ValidIssuer"], //el emisor debe ser el mismo que el del token
                ValidAudience = Configuration["JWT:ValidAudience"], //el receptor debe ser el mismo que el del token
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            Configuration["JWT:Secret"])), //la clave secreta debe ser la misma que la del token
                //ClockSkew = TimeSpan.Zero//para que no haya diferencia de tiempo
            };
        });

        // Add cache filter
        services.AddResponseCaching();

        //Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("CorsRule", rule => { rule.AllowAnyHeader().AllowAnyMethod().WithOrigins("*"); });
        }); // para permitir que se conecte el backend con el forntend

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiReview", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme =  "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id= "Bearer"
                    }
                },
                new string[]{}
                }
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        //if (env.IsDevelopment())
        //{
        app.UseSwagger();
        app.UseSwaggerUI();
        //}

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseResponseCaching();

        app.UseCors("CorsRule");

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}