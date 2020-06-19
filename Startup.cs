using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Endevrian.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Http.Features;

namespace Endevrian
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;

            using (ApplicationDbContext db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>()))
            {
                db.Database.EnsureCreated();
                //db.Database.Migrate();
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddEntityFrameworkSqlite().AddDbContext<ApplicationDbContext>();

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddControllers();
            services.AddProgressiveWebApp();
            services.AddMvc(options => options.EnableEndpointRouting = false).AddControllersAsServices();

            // To list physical files from a path provided by configuration:
            //PhysicalFileProvider physicalProvider = new PhysicalFileProvider(Configuration.GetValue<string>("StoredFilesPath"));
            //PhysicalFileProvider physicalProvider = new PhysicalFileProvider(environment);
            PhysicalFileProvider physicalProvider = new PhysicalFileProvider(Configuration.GetValue<string>(WebHostDefaults.ContentRootKey));
            services.AddSingleton<IFileProvider>(physicalProvider);
            services.AddDirectoryBrowser();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            string userContentPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserContent");

            if (!Directory.Exists(userContentPath))
            {
                Directory.CreateDirectory(userContentPath);
            }

            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(userContentPath),
                RequestPath = "/UserContent",
                EnableDirectoryBrowsing = true
            });

            //This will control the max file upload size. It probably needs to be 100 MB
            app.Use(async (context, next) =>
            {
                context.Features.Get<IHttpMaxRequestBodySizeFeature>()
                    .MaxRequestBodySize = null;

                await next.Invoke();
            });

            // Set up custom content types - associating file extension to MIME type
            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            //provider.Mappings[".myapp"] = "application/x-msdownload";
            //provider.Mappings[".htm3"] = "text/html";
            //provider.Mappings[".image"] = "image/png";
            // Replace an existing mapping
            //provider.Mappings[".rtf"] = "application/x-msdownload";
            // Remove MP4 videos.
            provider.Mappings.Remove(".html");

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                  name: "Identity",
                  template: "{area:exists}/{controller=User}/{action}/{id?}");

                routes.MapRoute(
                   name: "default",
                   template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
