using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rezolver;
using Rezolver.Options;

namespace Rezolver.Examples.AspNetCore._2._0
{
    // <example>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // you need AddOptions if you intend to use the configuration callback
            // in your call to UseRezolver() in program.cs
            services.AddOptions();

            services.AddMvc();
        }

        // Adding this method (even if empty) is enough to trigger the use of Rezolver
        // as the Asp.Net Core 2 Application's DI container.
        public void ConfigureContainer(ITargetContainer targets)
        {
            // The targets passed here will be used to create the ScopedContainer.

            // Perform additional Rezolver-specific registrations (e.g. Decoration) here.
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
    // </example>
}
