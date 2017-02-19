using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rezolver.Logging;

namespace Rezolver.Examples.AspnetCore
{
	public class Startup
	{
		private ILoggerFactory _loggerFactory;

		public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();

			//_loggerFactory is captured early to support the ConfigureDevelopment_LoggingServices
			//version.
			_loggerFactory = loggerFactory;
		}

		public IConfigurationRoot Configuration { get; }

		// <example>
		// This method gets called by the runtime. Use this method to add services to the container.
		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			// Add framework services.
			services.AddMvc();

			//create the Rezolver container from the services, without making any additional
			//registrations
			var container = services.CreateRezolverContainer();
			//use IContainer/ITargetContainer methods and extensions here to add extra registrations, e.g.
			//decorators or your own application's registrations.

			//IContainers implement the IServiceProvider interface natively - so can simply be returned.
			return container;
		}
		// </example>

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			_loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			_loggerFactory.AddDebug(LogLevel.Debug);
			//slight modification to the 'standard' template's environment check purely 
			//to cater for the additional environments that we have configured for these Rezolver demos.
			if (env.IsDevelopment() || env.EnvironmentName.StartsWith("Development"))
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
