using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace APICotacoes
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var serviceBusConfigurations = new ServiceBusConfigurations();
            new ConfigureFromConfigurationOptions<ServiceBusConfigurations>(
                Configuration.GetSection("ServiceBusConfigurations"))
                    .Configure(serviceBusConfigurations);
            services.AddSingleton(serviceBusConfigurations);

            services.AddMvc();

            // Ativando o Application Insights
            services.AddApplicationInsightsTelemetry(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}