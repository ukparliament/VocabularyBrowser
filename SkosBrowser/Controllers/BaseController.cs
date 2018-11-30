namespace WebApplication1
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using VDS.RDF;
    using VDS.RDF.Query;
    using VDS.RDF.Storage;

    public abstract class BaseController : Controller
    {
        protected IGraph Execute(string sparql)
        {
            var pp = new SparqlParameterizedString(sparql);

            return Execute(pp);
        }

        protected IGraph Execute(string sparql, Uri u)
        {
            var pp = new SparqlParameterizedString(sparql);

            if (!(u is null))
            {
                pp.SetUri("parameter", u);
            }

            return Execute(pp);
        }

        protected IGraph Execute(string sparql, string p)
        {

            var pp = new SparqlParameterizedString(sparql);

            if (!(p is null))
            {
                pp.SetLiteral("parameter", p);
            }

            return Execute(pp);
        }

        protected IGraph Execute(SparqlParameterizedString pp)
        {
            var connector = new SparqlConnector(new Uri("https://opengraphdb.azurewebsites.net/repositories/vocabulary"));

            return connector.Query(pp.ToString()) as IGraph;
        }
    }
}
