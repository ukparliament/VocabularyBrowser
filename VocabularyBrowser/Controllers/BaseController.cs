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
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using VDS.RDF;

    public class BaseController : Controller
    {
        public BaseController(VocabularyService vocabularyService)
        {
            this.VocabularyService = vocabularyService;
            this.SchemeList = this.GetSchemeList();
        }

        protected VocabularyService VocabularyService { get; set; }

        protected SelectList SchemeList { get; set; }

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
