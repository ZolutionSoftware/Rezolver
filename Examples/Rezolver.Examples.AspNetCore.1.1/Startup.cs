using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Rezolver.Examples.AspNetCore._1._1
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }

		
		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services.
			services.AddMvc();
		}

        // TODO: Show this alternative (older) way to configure Rezolver.
        //// <example2>
        //// This method gets called by the runtime. Use this method to add services to the container.
        //public IServiceProvider ConfigureServices(IServiceCollection services)
        //{
        //    // Add framework services.
        //    services.AddMvc();

        //    //create the Rezolver container from the services, without making any additional
        //    //registrations
        //    var container = services.CreateRezolverContainer();
        //    //use IContainer/ITargetContainer methods and extensions here to add extra registrations, e.g.
        //    //decorators or your own application's registrations.

        //    //IContainers implement the IServiceProvider interface natively - so can simply be returned.
        //    return container;
        //}
        //// </example2>

        // <example>
        public void ConfigureContainer(ITargetContainer container)
		{
			//by declaring this method - even if empty - you trigger the 
			//creation of the ITargetContainer which will ultimately
			//be used to create the IContainer that will be used
			//as the application's container.

			//Here you can perform additional registrations/configuration on the
			//ITargetContainer here.
		}
		// </example>

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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
}
