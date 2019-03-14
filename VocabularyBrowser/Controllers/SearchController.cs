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
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.Extensions.Configuration;
    using VDS.RDF;
    using VDS.RDF.Dynamic;
    using VDS.RDF.Query;
    using VocabularyBrowser.Models;

    [Route("vocabulary/browser/search")]
    public class SearchController : BaseController
    {
        public SearchController(VocabularyService vocabularyService, IConfiguration config)
            : base(vocabularyService)
        {
            this.LuceneConnector = config.GetSection("SearchController")["luceneConnector"];
        }

        protected string LuceneConnector { get; set; }

        [HttpGet]
        public ActionResult Query(string searchText)
        {
            return Query(new SearchQuery() { SearchText = searchText });
        }

        [HttpPost]
        public ActionResult Query(SearchQuery searchQuery)
        {
            searchQuery.ConceptSchemeList = GetSchemeList();

            if (string.IsNullOrWhiteSpace(searchQuery.SearchText))
            {
                ModelState.AddModelError(string.Empty, "search text is required");
                return View(searchQuery);
            }

            string sparqlFilter = string.Empty;
            if (searchQuery.ExactMatch)
            {
                sparqlFilter = @"FILTER(LCASE(STR(?prefLabel)) = LCASE(@query))";
            }
            string schemeBind = string.Empty;
            if (searchQuery.ConceptScheme != null && !searchQuery.ConceptScheme.Equals("-1"))
            {
                schemeBind = $"BIND(<{searchQuery.ConceptScheme}> as ?scheme)";
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

    {schemeBind}
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

            var connector = new Uri(this.LuceneConnector);

            var pp = new SparqlParameterizedString(sparql);
            pp.SetUri("connector", connector);
            pp.SetLiteral("query", searchQuery.SearchText);

            searchQuery.Results = new DynamicGraph(this.VocabularyService.Execute(pp), subjectBaseUri: new Uri("urn:"));

            return this.View(searchQuery);
        }

        private SelectList GetSchemeList()
        {
            var sparql = @"
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT {
    ?scheme
        skos:prefLabel ?label ;
    .
}
WHERE {
    ?scheme
        a skos:ConceptScheme ;
        skos:prefLabel ?prefLabel ;
    .

    BIND(STR(?prefLabel) AS ?label)
}
";
            var result = this.VocabularyService.Execute(sparql);
            var lst = result.Triples.Select(x => new { (x.Subject as UriNode).Uri.AbsoluteUri, (x.Object as LiteralNode).Value })
                .Distinct()
                .ToList();
            lst.Insert(0, new { AbsoluteUri = "-1", Value = "All schemes" });
            return new SelectList(lst.OrderBy(i => i.Value), "AbsoluteUri", "Value");
        }
    }
}
