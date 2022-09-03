using ApiGateway.Aggregator;
using ApiGateway.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

namespace ApiGateway
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            /*var secretKey = Configuration.GetValue<string>("JwtOptions:SecretKey");
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                        ClockSkew = new System.TimeSpan(0)
                    };
                });*/

            services.AddOcelot()
                .AddDelegatingHandler<LogDelegatingHandler>(true)
                .AddSingletonDefinedAggregator< PostUsuarioAggregator>();



            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ejemplo Api Gateway", Version = "v1" });
            });
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {           

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ejemplo Api Gateway v1"));
            }

            app.UseOcelot().Wait();

        }
    }
}
