namespace WebApplication1
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using VDS.RDF.Dynamic;
    using VDS.RDF.Query;

    [Route("search")]
    public class SearchController : Controller
    {
        private readonly VocabularyService vocabularyService;
        private readonly string indexName;

        public SearchController(VocabularyService vocabularyService, IConfiguration config)
        {
            this.vocabularyService = vocabularyService;
            this.indexName = config.GetSection("SearchController")["indexName"];
        }

        [HttpGet]
        public ActionResult Query(string q)
        {
            var sparql = @"
PREFIX : <urn:>
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT
{
    :result :member ?result .

    ?result
        :type ?type ;
        :label ?label ;
        :collection ?collection ;
        :scheme ?scheme ;
    .

    ?collection
        :label ?collectionLabel ;
    .

    ?scheme
        :label ?schemeLabel ;
    .
}
FROM <http://www.ontotext.com/explicit>
WHERE { 
    OPTIONAL {
        ?value @indexUri @query . 

        ?result
            ?p ?value ;
            a ?type ;
            skos:prefLabel ?prefLabel ;
        .

        OPTIONAL {
            ?collection
                skos:member ?result ;
                skos:prefLabel ?collectionPrefLabel ;
            .
            
            BIND(STR(?collectionPrefLabel) AS ?collectionLabel)
        }

        OPTIONAL {
            ?scheme
                ^skos:inScheme ?result ;
                skos:prefLabel ?schemePrefLabel ;
            .
            
            BIND(STR(?schemePrefLabel) AS ?schemeLabel)
        }

        BIND(STR(?prefLabel) AS ?label)
    }
}
";

            var indexUri = new Uri(new Uri("http://www.ontotext.com/owlim/lucene"), $"#{this.indexName}");

            var pp = new SparqlParameterizedString(sparql);
            pp.SetUri("indexUri", indexUri);
            pp.SetLiteral("query", q);

            this.ViewData["Query"] = q;

            return this.View(new DynamicGraph(this.vocabularyService.Execute(pp), subjectBaseUri: new Uri("urn:")));
        }
    }
}
