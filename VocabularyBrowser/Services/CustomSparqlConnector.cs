namespace VDS.RDF.Storage
{
    using VDS.RDF.Query;

    public class CustomSparqlConnector : SparqlConnector
    {
        public CustomSparqlConnector(CustomSparqlRemoteEndpoint endpoint) : base(endpoint)
        {

        }
    }
}
