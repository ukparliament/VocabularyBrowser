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
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.Extensions.Configuration;
    using VDS.RDF;
    using VDS.RDF.Dynamic;
    using VDS.RDF.Query;
    using VocabularyBrowser.Models;

    [Route("vocabulary/browser/api/")]
    [ApiController]
    public class SearchApiController : BaseController
    {
        public SearchApiController(VocabularyService vocabularyService, IConfiguration config)
            : base(vocabularyService)
        {
            this.LuceneConnector = config.GetSection("SearchController")["luceneConnector"];
        }

        protected string LuceneConnector { get; set; }

        [HttpGet("search")]
        [HttpGet("search.{format}")]
        [FormatFilter]
        public ActionResult<Feed> Query(string q, string scheme = null)
        {
            var entries = new List<Entry>();

            if (q == null || q.Length < 2)
            {
                return new Feed() { Entries = entries };
            }

            string schemeBind = string.Empty;
            if (scheme != null && !scheme.Equals("-1"))
            {
                schemeBind = $"BIND(id:{scheme} as ?scheme)";
            }

            var sparql = $@"
PREFIX : <urn:>
PREFIX luc: <http://www.ontotext.com/connectors/lucene#>
PREFIX inst: <http://www.ontotext.com/connectors/lucene/instance#>
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>
PREFIX id: <https://id.parliament.uk/>

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
}}
";

            var connector = new Uri(this.LuceneConnector);

            var pp = new SparqlParameterizedString(sparql);
            pp.SetUri("connector", connector);
            pp.SetLiteral("query", q);

            var graph = new DynamicGraph(this.VocabularyService.Execute(pp), subjectBaseUri: new Uri("urn:"));

            if (graph != null && graph.TryGetValue("result", out dynamic results))
            {
                foreach (var entry in results.member.OrderByDescending((Func<dynamic, dynamic>)(x => x.score.Single())))
                {
                    entries.Add(new Entry() { Id = entry.ToString(), Title = entry.label.Single() });
                }
            }

            return new Feed() { Entries = entries };
        }
    }
}
