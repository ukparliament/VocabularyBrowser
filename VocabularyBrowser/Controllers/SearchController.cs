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
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using VDS.RDF.Dynamic;
    using VDS.RDF.Query;
    using VocabularyBrowser.Models;

    [Route("vocabulary/browser/search")]
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
        public ActionResult Query(SearchQuery searchQuery)
        {
            // TODO: When this query has no result,
            // an empty graph is returned.
            // Newer version of GraphDB send no content type header.
            // dotnetrdf fails to find a parser for a response without content type.
            // Quick fix is to always have something in the result graph.
            // Or to add a default header.
            // Fix is to modify dnr parser selection.
            string sparqlFilter = "";
            if (searchQuery.ExactMatch)
            {
                sparqlFilter = @"FILTER(LCASE(STR(?prefLabel)) = LCASE(@query))";
            }
            var sparql = $@"
PREFIX : <urn:>
PREFIX luc: <http://www.ontotext.com/connectors/lucene#>
PREFIX inst: <http://www.ontotext.com/connectors/lucene/instance#>
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT {{
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
}}
WHERE
{{
    {{
        SELECT ?entity ?score 
        WHERE {{
            ?search
                a @connector ;
                luc:query @query ;
                luc:entities ?entity ;
            .

            ?entity luc:score ?score .
        }}
    }}

    ?entity
        a skos:Concept ;
        skos:prefLabel ?prefLabel ;
        skos:inScheme ?scheme .

    ?scheme skos:prefLabel ?schemePrefLabel .

    OPTIONAL {{
        ?collection
            skos:member ?entity ;
            skos:prefLabel ?collectionPrefLabel ;
        .
        BIND(STR(?collectionPrefLabel) AS ?collectionLabel)
    }}

    BIND(STR(?prefLabel) AS ?label)
    BIND(STR(?schemePrefLabel) AS ?schemeLabel)
    {sparqlFilter}
}}
";

            var connector = new Uri(this.luceneConnector);

            var pp = new SparqlParameterizedString(sparql);
            pp.SetUri("connector", connector);
            pp.SetLiteral("query", searchQuery.SearchText);

            this.ViewData["Query"] = searchQuery.SearchText;

            return this.View(new DynamicGraph(this.vocabularyService.Execute(pp), subjectBaseUri: new Uri("urn:")));
        }
    }
}
