namespace WebApplication1
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using VDS.RDF.Dynamic;

    [Route("search")]
    public class SearchController : Controller
    {
        private readonly VocabularyService vocabularyService;

        public SearchController(VocabularyService vocabularyService)
        {
            this.vocabularyService = vocabularyService;
        }

        [HttpGet]
        public ActionResult Query(string q)
        {
            var sparql = @"
PREFIX : <urn:>
PREFIX luc: <http://www.ontotext.com/owlim/lucene#>
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT
{
    :query :value ?query .

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
    BIND(@parameter AS ?query)

    OPTIONAL {
        ?value luc:myIndex ?query . 

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

            return this.View(new DynamicGraph(this.vocabularyService.Execute(sparql, q), subjectBaseUri: new Uri("urn:")));
        }
    }
}
