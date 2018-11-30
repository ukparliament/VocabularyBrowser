namespace WebApplication1
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    public class CollectionController : BaseController
    {
        [Route("collections")]
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
ORDER BY ?prefLabel
";

            return this.View(new Skos(this.Execute(sparql)));
        }

        [Route("collections/{id}")]
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
ORDER BY ?conceptPrefLabel
";

            var skos = new Skos(this.Execute(sparql, new Uri(Program.BaseUri, id)));
            var collection = skos.Collections.SingleOrDefault(c => c.Id == id);
                
            if (collection is null)
            {
                return NotFound();
            }

            return View(collection);
        }
    }
}
