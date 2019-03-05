namespace VDS.RDF.Query
{
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Net;

    public class CustomSparqlRemoteEndpoint : SparqlRemoteEndpoint
    {
        private readonly string apiManagementSubscriptionKey;

        public CustomSparqlRemoteEndpoint(Uri endpointUri, IConfiguration config) : base(endpointUri)
        {
            this.apiManagementSubscriptionKey = config.GetSection("VocabularyService")["ProductSubscriptionKey"];
        }

        protected override void ApplyCustomRequestOptions(HttpWebRequest httpRequest)
        {
            if (!string.IsNullOrEmpty(this.apiManagementSubscriptionKey))
            {
                httpRequest.Headers["Ocp-Apim-Subscription-Key"] = this.apiManagementSubscriptionKey;
            }
        }
    }
}
