using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.DependencyInjection.Rezolver;

namespace Rezolver.Examples.AspNet5
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.
            Configuration = new ConfigurationBuilder(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddEnvironmentVariables().Build();
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration,"AppSettings");

            // Add MVC services to the services container.
            services.AddMvc();

            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();

            var provider = services.BuildServiceProvider();

            //see the Asp.Net MVC 6 sample in github - early resolving of the application environment
            //enables us to see if we have a rezolver.json file.
            var appEnv = provider.GetRequiredService<IApplicationEnvironment>();
            //note - the code below does not work for DNX451, because the assembly target compiler produces compiled
            //code that is denied access to at least one constructor that is being used by the standard set of service registrations
//            IRezolveTargetCompiler compiler = null;
//#if DNX451
//            compiler = new AssemblyRezolveTargetCompiler();
//#else
//            compiler = new RezolveTargetDelegateCompiler();
//#endif
            //so we forced to use the default compiler, which compiles to in-memory delegates
            var rezolver = new DefaultLifetimeScopeRezolver();

            rezolver.Populate(services);

            provider = rezolver;


            return provider;
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            // Configure the HTTP request pipeline.

            // Add the console logger.
            //commented out as this is causing a runtime reflection exception 
            //loggerfactory.AddConsole();

            // Add the following to the request pipeline only in development environment.
            if (env.IsEnvironment("Development"))
            {
                //commented out as this is causing a runtime reflection exception 
                //app.UseBrowserLink();
                app.UseErrorPage(ErrorPageOptions.ShowAll);
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // send the request to the following path or controller action.
                app.UseErrorHandler("/Home/Error");
            }

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                // Uncomment the following line to add a route for porting Web API 2 controllers.
                // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
            });
        }
    }
}
