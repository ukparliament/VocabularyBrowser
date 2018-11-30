namespace WebApplication1
{
    using System.Collections.Generic;
    using SolrNet.Attributes;

    public class SolrResult
    {
        [SolrField("title_t")]
        public string Title { get; set; }

        [SolrField("externalLocation_uri")]
        public IEnumerable<string> ExternalLocation { get; set; }

        [SolrField("type_ses")]
        public IEnumerable<string> ContentTypeIds { get; set; }

        public IEnumerable<Concept> ContentTypes { get; set; }
    }
}
