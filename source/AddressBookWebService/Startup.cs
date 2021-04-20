using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharedLib.Services;

namespace AddressBookWebService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            //var addressBookSettings = new AddressBookSettings();
            //Configuration.GetSection("AddressBookSettings").Bind(addressBookSettings);

            services.Configure<AddressBookSettings>(Configuration.GetSection("AddressBookSettings")); //hot reloadable

            string filenameWhereToStoreReceiveAddresses = Configuration.GetValue<string>("AddressBookSettings:FilenameWhereToStoreReceiveAddresses");
            string goShimmerDashboardUrl = Configuration.GetValue<string>("AddressBookSettings:GoShimmerDashboardUrl");
            services.AddSingleton<IAddressService>(x => new AddressService(filenameWhereToStoreReceiveAddresses, goShimmerDashboardUrl));

            string filenameWhereToStoreNodeUrls = Configuration.GetValue<string>("AddressBookSettings:FilenameWhereToStoreNodeUrls");
            services.AddSingleton<INodeService>(x => new NodeService(filenameWhereToStoreNodeUrls));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                Console.WriteLine("Before Invoke from 1st app.Use()\n");
                await next();
                Console.WriteLine("After Invoke from 1st app.Use()\n");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
