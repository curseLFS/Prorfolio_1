using KioskWebService.Controllers;
using KioskWebService.Helpers;
using KioskWebService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace KioskWebService
{
    public class Startup
    {
        readonly string AllowSpecificOrigins = "_allowSpecificOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;           
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // configuration basic authentication
            services.AddAuthentication("BasicAuthentication")
               .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            services.AddScoped<IUserService, UserService>();

            services.AddTransient<KioskController>();
     
            services.AddControllers();

            var fromServer = Configuration.GetSection("AllowFromServer");
            var fromLocalHost = Configuration.GetSection("AllowFromLocalHost");
            var fromDev = Configuration.GetSection("AllowFromDevServer");
            var fromTQA = Configuration.GetSection("AllowFromTQAServer");
            var fromPreProd = Configuration.GetSection("AllowFromPreProdServer");
            var fromProd = Configuration.GetSection("AllowFromProdServer");

            var fromServer1 = Serialize(fromServer).ToString().Split(';');
            var fromLocalHost1 = Serialize(fromLocalHost).ToString().Split(';');
            var fromDev1 = Serialize(fromDev).ToString().Split(';');
            var fromTQA1 = Serialize(fromTQA).ToString().Split(';');
            var fromPreProd1 = Serialize(fromPreProd).ToString().Split(';');
            var fromProd1 = Serialize(fromProd).ToString().Split(';');

            var concatAddress = fromServer1.Concat(fromLocalHost1)
                                           .Concat(fromDev1)
                                           .Concat(fromTQA1)
                                           .Concat(fromPreProd1)
                                           .Concat(fromProd1).ToArray();


            services.AddCors(options => 
            {
                options.AddPolicy(name: AllowSpecificOrigins,
                    builder => 
                    {
                        builder.WithOrigins
                        (
                            concatAddress
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
                options.HttpsPort = 443;
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
            });

            services.AddProgressiveWebApp();
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else 
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();           
            app.UseStaticFiles();
            app.UseRouting();           
            app.UseSerilogRequestLogging();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());            
        }

        private JToken Serialize(IConfiguration config)
        {
            JObject obj = new JObject();
            foreach (var child in config.GetChildren())
            {
                obj.Add(child.Key, Serialize(child));
            }

            if (!obj.HasValues && config is IConfigurationSection section)
                return new JValue(section.Value);

            return obj;
        }
    }
}
