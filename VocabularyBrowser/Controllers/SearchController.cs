namespace VocabularyBrowser
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
        private readonly string luceneConnector;

        public SearchController(VocabularyService vocabularyService, IConfiguration config)
        {
            this.vocabularyService = vocabularyService;
            this.luceneConnector = config.GetSection("SearchController")["luceneConnector"];
        }

        [HttpGet]
        public ActionResult Query(string q)
        {
            var sparql = @"
PREFIX : <urn:>
PREFIX luc: <http://www.ontotext.com/connectors/lucene#>
PREFIX inst: <http://www.ontotext.com/connectors/lucene/instance#>
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT {
    :result :member ?entity .
    
    ?entity
        :score ?score ;
        :type skos:Concept ;
        :label ?label ;
        :collection ?collection ;
        :scheme ?scheme ;
    .
    
    ?collection :label ?collectionLabel .
    
    ?scheme :label ?schemeLabel .
}
WHERE
{
    {
        SELECT ?entity ?score 
        WHERE {
            ?search
                a @connector ;
                luc:query @query ;
                luc:entities ?entity ;
            .

            ?entity luc:score ?score .
        }
    }

    ?entity
        a skos:Concept ;
        skos:prefLabel ?prefLabel ;
        skos:inScheme ?scheme .

    ?scheme skos:prefLabel ?schemePrefLabel .

    OPTIONAL {
        ?collection
            skos:member ?entity ;
            skos:prefLabel ?collectionPrefLabel ;
        .
        BIND(STR(?collectionPrefLabel) AS ?collectionLabel)
    }

    BIND(STR(?prefLabel) AS ?label)
    BIND(STR(?schemePrefLabel) AS ?schemeLabel)
}
";

            var connector = new Uri(this.luceneConnector);

            var pp = new SparqlParameterizedString(sparql);
            pp.SetUri("connector", connector);
            pp.SetLiteral("query", q);

            this.ViewData["Query"] = q;

            return this.View(new DynamicGraph(this.vocabularyService.Execute(pp), subjectBaseUri: new Uri("urn:")));
        }
    }
}
