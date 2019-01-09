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
    using VDS.RDF;

    [Route("vocabulary/browser/schemes")]
    public class ConceptSchemeController : Controller
    {
        private readonly VocabularyService vocabularyService;

        public ConceptSchemeController(VocabularyService vocabularyService)
        {
            this.vocabularyService = vocabularyService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var sparql = @"
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT {
    ?scheme
        a skos:ConceptScheme ;
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

            return this.View(new Skos(this.vocabularyService.Execute(sparql)));
        }

        [HttpGet("{id}")]
        public ActionResult Item(string id)
        {
            var sparql = @"
PREFIX : <urn:>
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT {
    ?scheme
        a skos:ConceptScheme ;
        skos:prefLabel ?schemeLabel ;
        skos:hasTopConcept ?concept ;
    .

    ?concept
        a skos:Concept ;
        skos:prefLabel ?conceptLabel ;
    .
}
WHERE {
    BIND(@parameter AS ?scheme)

    ?scheme
        a skos:ConceptScheme ;
        skos:prefLabel ?schemePrefLabel ;
        skos:hasTopConcept ?concept ;
    .

    ?concept
        skos:prefLabel ?conceptPrefLabel ;
    .

    BIND(STR(?schemePrefLabel) AS ?schemeLabel)
    BIND(STR(?conceptPrefLabel) AS ?conceptLabel)
}
";

            var graph = new Skos(this.vocabularyService.Execute(sparql, new Uri(Program.BaseUri, id)));
            return this.View(graph.ConceptSchemes.Single(cs => cs.Id == id));
        }

        [HttpGet("{id}/tree")]
        public ActionResult Tree(string id)
        {
            var sparql = @"
PREFIX : <urn:>
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT {
    ?scheme
        a skos:ConceptScheme ;
        skos:prefLabel ?schemeLabel ;
        skos:hasTopConcept ?topConcept ;
    .

    ?concept
        a skos:Concept ;
        skos:narrower ?narrower ;
        skos:prefLabel ?conceptLabel ;
    .
}
WHERE {
    BIND(@parameter AS ?scheme)

    {
        ?scheme
            a skos:ConceptScheme ;
            skos:prefLabel ?schemePrefLabel ;
            skos:hasTopConcept ?topConcept ;
        .

        BIND(STR(?schemePrefLabel) AS ?schemeLabel)
    }
    UNION
    {
        ?concept
            skos:inScheme ?scheme ;
            skos:prefLabel ?conceptPrefLabel ;
        .

        BIND(STR(?conceptPrefLabel) AS ?conceptLabel)

        OPTIONAL {
            ?narrower
                ^skos:narrower ?concept ;
                skos:inScheme ?scheme ;
            .
        }
    }
}
";

            var g = new Skos(this.vocabularyService.Execute(sparql, new Uri(Program.BaseUri, id)));

            foreach (var scheme in g.ConceptSchemes)
            {
                foreach (var topConcept in scheme.HasTopConcept)
                {
                    CutCycles(topConcept);
                }
            }

            return this.View(g.ConceptSchemes.Single(cs => cs.Id == id));
        }

        private static void CutCycles(Concept concept, Stack<INode> seen = null)
        {
            if (seen is null)
            {
                seen = new Stack<INode>();
            }

            seen.Push(concept);

            foreach (var narrower in concept.Narrower.ToArray())
            {
                if (seen.Contains(narrower))
                {
                    concept.Narrower.Remove(narrower);
                }
                else
                {
                    CutCycles(narrower, seen);
                }
            }

            seen.Pop();
        }
    }
}
