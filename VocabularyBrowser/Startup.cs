namespace VocabularyBrowser
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using SolrNet;

    public class Startup
    {
        private readonly string solrNetUrl;

        public Startup(IConfiguration config)
        {
            this.solrNetUrl = config.GetSection("SolrNet")["Url"];
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddSingleton<VocabularyService>();
            services.AddMvc().AddViewLocalization();
            services.AddSolrNet(this.solrNetUrl);
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
