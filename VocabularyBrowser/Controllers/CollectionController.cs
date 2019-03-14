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

    [Route("vocabulary/browser/collections")]
    public class CollectionController : BaseController
    {
        public CollectionController(VocabularyService vocabularyService)
            : base(vocabularyService)
        {
        }

        [HttpGet]
        public ActionResult Index()
        {
            var sparql = @"
PREFIX : <urn:>
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT {
    ?collection 
        a skos:Collection ;
        skos:prefLabel ?label ;
    .
}
WHERE {
    ?collection
        a skos:Collection ;
        skos:prefLabel ?prefLabel ;
    .

    BIND(STR(?prefLabel) AS ?label)
}
";
            return this.View(new Skos(this.VocabularyService.Execute(sparql)));
        }

        [HttpGet("{id}")]
        public ActionResult Item(string id)
        {
            var sparql = @"
PREFIX : <urn:>
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT {
    ?collection
        <urn:selected> true ;
        a skos:Collection ;
        skos:prefLabel ?collectionLabel ;
        skos:member ?concept ;
        skos:member ?child ;
    .

    ?concept
        a skos:Concept ;
        skos:prefLabel ?conceptLabel ;
    .

    ?parent
        a skos:Collection ;
        skos:member ?collection ;
        skos:prefLabel ?parentLabel ;
    .

    ?child
        a skos:Collection ;
        skos:prefLabel ?childLabel ;
    .
}
WHERE {
    BIND(@parameter AS ?collection)

    ?collection
        skos:prefLabel ?collectionPrefLabel ;
    .

    BIND(STR(?collectionPrefLabel) AS ?collectionLabel)

    OPTIONAL {
        ?concept
            a skos:Concept ;
            ^skos:member ?collection ;
            skos:prefLabel ?conceptPrefLabel ;
        .

        BIND(STR(?conceptPrefLabel) AS ?conceptLabel)
    }

    OPTIONAL {
        ?parent
            a skos:Collection ;
            skos:member ?collection ;
            skos:prefLabel ?parentPrefLabel ;
        .

        BIND(STR(?parentPrefLabel) AS ?parentLabel)
    }

    OPTIONAL {
        ?child
            a skos:Collection ;
            ^skos:member ?collection ;
            skos:prefLabel ?childPrefLabel ;
        .

        BIND(STR(?childPrefLabel) AS ?childLabel)
    }
}
";

            var skos = new Skos(this.VocabularyService.Execute(sparql, new Uri(Program.BaseUri, id)));
            var collection = skos.Collections.SingleOrDefault(c => c.Id == id);

            if (collection is null)
            {
                return NotFound();
            }

            return View(collection);
        }
    }
}
