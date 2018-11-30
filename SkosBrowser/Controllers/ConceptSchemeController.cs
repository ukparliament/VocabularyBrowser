namespace WebApplication1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using VDS.RDF;

    public class ConceptSchemeController : BaseController
    {
        [Route("schemes")]
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
ORDER BY ?prefLabel
";

            return this.View(new Skos(this.Execute(sparql)));
        }

        [Route("schemes/{id}")]
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
ORDER BY ?conceptPrefLabel
";

            var graph = new Skos(this.Execute(sparql, new Uri(Program.BaseUri, id)));
            return this.View(graph.ConceptSchemes.Single(cs => cs.Id == id));
        }

        [Route("schemes/{id}/tree")]
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
ORDER BY ?conceptPrefLabel
";

            var g = new Skos(this.Execute(sparql, new Uri(Program.BaseUri, id)));

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
