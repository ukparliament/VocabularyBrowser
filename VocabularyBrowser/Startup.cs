// MIT License
//
// Copyright (c) 2019 UK Parliament
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace VocabularyBrowser
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using SolrNet;

    public class Startup
    {
        private readonly string solrNetUrl;
        private readonly string solrNetSubscriptionKey;

        public Startup(IConfiguration config)
        {
            this.solrNetUrl = config.GetSection("SolrNet")["Url"];
            this.solrNetSubscriptionKey = config.GetSection("SolrNet")["SubscriptionKey"];
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddSingleton<VocabularyService>();
            services.AddMvc(this.ConfigureMvc).AddViewLocalization();
            services.AddSolrNet(this.solrNetUrl, this.SetupSolr);
        }

        private void ConfigureMvc(MvcOptions mvcOptions)
        {
            mvcOptions.RespectBrowserAcceptHeader = true;
            mvcOptions.OutputFormatters.Add(new XmlSerializerOutputFormatter());
            mvcOptions.FormatterMappings.SetMediaTypeMappingForFormat("xml", "text/xml");
            mvcOptions.FormatterMappings.SetMediaTypeMappingForFormat("json", "application/json");
        }

        private void SetupSolr(SolrNetOptions options)
        {
            if (!string.IsNullOrEmpty(this.solrNetSubscriptionKey))
            {
                options.HttpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.solrNetSubscriptionKey);
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRewriter(new RewriteOptions().AddRedirect("^$", "vocabulary/browser"));
            app.UseMvc();
        }
    }
}
