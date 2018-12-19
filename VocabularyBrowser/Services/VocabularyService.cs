namespace VocabularyBrowser
{
    using System;
    using Microsoft.Extensions.Configuration;
    using VDS.RDF;
    using VDS.RDF.Query;
    using VDS.RDF.Storage;

    public class VocabularyService : IDisposable
    {
        private readonly SparqlConnector connector;

        public VocabularyService(IConfiguration config)
        {
            this.connector = new SparqlConnector(new Uri(config.GetSection("VocabularyService")["SparqlEndpoint"]));
        }

        internal IGraph Execute(string sparql)
        {
            return connector.Query(sparql) as IGraph;
        }

        internal IGraph Execute(string sparql, Uri u)
        {
            var pp = new SparqlParameterizedString(sparql);

            if (!(u is null))
            {
                pp.SetUri("parameter", u);
            }

            return this.Execute(pp);
        }

        internal IGraph Execute(string sparql, string p)
        {

            var pp = new SparqlParameterizedString(sparql);

            if (!(p is null))
            {
                pp.SetLiteral("parameter", p);
            }

            return this.Execute(pp);
        }

        internal IGraph Execute(SparqlParameterizedString pp)
        {
            return this.Execute(pp.ToString());
        }

        void IDisposable.Dispose()
        {
            this.connector.Dispose();
        }
    }
}
