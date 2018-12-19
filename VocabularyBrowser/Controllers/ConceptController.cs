namespace VocabularyBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using SolrNet;
    using SolrNet.Commands.Parameters;
    using VDS.RDF;
    using VDS.RDF.Dynamic;
    using VDS.RDF.Query;
    using VDS.RDF.Writing.Formatting;

    [Route("concepts")]
    public class ConceptController : Controller
    {
        private readonly ISolrOperations<SolrResult> solr;
        private readonly VocabularyService vocabularyService;

        public ConceptController(ISolrOperations<SolrResult> solr, VocabularyService vocabularyService)
        {
            this.solr = solr;
            this.vocabularyService = vocabularyService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var sparql = @"
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT {
    <urn:initials> <urn:value> ?firstLetter .
}
WHERE {
    SELECT DISTINCT ?firstLetter
    WHERE {
        ?concept
            a skos:Concept ;
            skos:prefLabel ?prefLabel ;
        .

        BIND(UCASE(SUBSTR(STR(?prefLabel), 1, 1)) AS ?firstLetter)
    }
}
";

            return this.View(new DynamicGraph(this.vocabularyService.Execute(sparql), new Uri("urn:")));
        }

        [HttpGet("startingwith/{prefix}")]
        public ActionResult StartingWith(string prefix)
        {
            var sparql = @"
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT {
    <urn:initials> <urn:value> ?firstLetter .
    ?concept
        a skos:Concept ;
        skos:prefLabel ?label .
}
WHERE {
    {
        SELECT DISTINCT ?firstLetter
        WHERE {
            ?concept
                a skos:Concept ;
                skos:prefLabel ?prefLabel ;
            .

            BIND(UCASE(SUBSTR(STR(?prefLabel), 1, 1)) AS ?firstLetter)
        }
    } 
    UNION
    {
        ?concept
            a skos:Concept ;
            skos:prefLabel ?prefLabel ;
        .

        FILTER(STRSTARTS(UCASE(?prefLabel), UCASE(@prefix)))
        BIND(STR(?prefLabel) AS ?label)
    }
}
";

            var pp = new SparqlParameterizedString(sparql);
            pp.SetLiteral("prefix", prefix);

            this.ViewData["prefix"] = prefix;
            return this.View(new Skos(this.vocabularyService.Execute(pp), new Uri("urn:")));
        }

        [HttpGet("{id}")]
        public ActionResult Item(string id)
        {
            var sparql = @"
PREFIX : <urn:>
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT {
    ?concept
        <urn:selected> true ;
        a skos:Concept ;
        skos:prefLabel ?conceptLabel ;
        skos:altLabel ?altLabel ;
        skos:definition ?definition ;
        skos:historyNote ?historyNote ;
        skos:notation ?notation ;
        skos:narrower ?narrower ;
        skos:broader ?broader ;
        skos:inScheme ?scheme ;
    .

    ?narrower
        a skos:Concept ;
        skos:prefLabel ?narrowerLabel ;
    .

    ?broader
        a skos:Concept ;
        skos:prefLabel ?broaderLabel ;
    .

    ?scheme
        a skos:ConceptScheme ;
        skos:prefLabel ?schemeLabel ;
    .

    ?parent
        a skos:Collection ;
        skos:member ?concept ;
        skos:prefLabel ?parentLabel ;
    .
}
WHERE {
    BIND(@parameter AS ?concept)

    ?concept
        a skos:Concept ;
        skos:prefLabel ?conceptPrefLabel ;
    .

    BIND(STR(?conceptPrefLabel) AS ?conceptLabel)

    OPTIONAL {
        ?concept skos:altLabel ?conceptAltLabel .

        BIND(STR(?conceptAltLabel) AS ?altLabel)
    }
    OPTIONAL { ?concept skos:definition ?definition . }
    OPTIONAL { ?concept skos:historyNote ?historyNote . }
    OPTIONAL { ?concept skos:notation ?notation . }

    OPTIONAL {
        ?narrower
            ^skos:narrower ?concept ;
            skos:prefLabel ?narrowerPrefLabel ;
        .

        BIND(STR(?narrowerPrefLabel) AS ?narrowerLabel)
    }

    OPTIONAL {
        ?broader
            ^skos:broader ?concept ;
            skos:prefLabel ?broaderPrefLabel ;
        .

        BIND(STR(?broaderPrefLabel) AS ?broaderLabel)
    }

    OPTIONAL {
        ?scheme
            ^skos:inScheme ?concept ;
            skos:prefLabel ?schemePrefLabel ;
        .

        BIND(STR(?schemePrefLabel) AS ?schemeLabel)
    }

    OPTIONAL {
        ?parent
            skos:member ?concept ;
            skos:prefLabel ?parentPrefLabel ;
        .

        BIND(STR(?parentPrefLabel) AS ?parentLabel)
    }
}
";

            var graph = new Skos(this.vocabularyService.Execute(sparql, new Uri(Program.BaseUri, id)));
            return this.View(graph.Concepts.Single(c => c.Id == id));
        }

        [HttpGet("{id}/entities")]
        public async Task<ActionResult> Entities(string id)
        {
            var results = await this.solr.QueryAsync(
                    $"all_ses:{id}",
                    new QueryOptions
                    {
                        Fields = new[] {
                            "title_t",
                            "externalLocation_uri",
                            "type_ses"
                        },
                        Rows = 100
                    }
                );

            var contentTypes = results.SelectMany(r => r.ContentTypeIds).Distinct();
            if (contentTypes.Any())
            {
                var concepts = this.GetConcepts(contentTypes).Concepts;

                results.ForEach(result =>
                    result.ContentTypes = result.ContentTypeIds.SelectMany(contentTypeId =>
                        concepts.Where(concept =>
                            concept.Id == contentTypeId)));
            }

            this.ViewData["label"] = GetConcepts(new[] { id }).Concepts.Single().PrefLabel.Single();
            this.ViewData["id"] = id;
            return this.View(results);
        }

        private Skos GetConcepts(IEnumerable<string> ids)
        {
            var sparql = @"
PREFIX : <urn:>
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

CONSTRUCT {
    ?concept
        a skos:Concept ;
        skos:prefLabel ?label .
}
WHERE {
    VALUES ?concept { @concepts }

    ?concept
        a skos:Concept ;
        skos:prefLabel ?prefLabel ;
    .

    BIND(STR(?prefLabel) AS ?label)
}
";

            return new Skos(this.vocabularyService.Execute(SetUris(sparql, ids)));
        }

        private static string SetUris(string sparql, IEnumerable<string> ids)
        {
            var formatter = new SparqlFormatter();
            var factory = new NodeFactory();

            string format(string id)
            {
                var uri = new Uri(Program.BaseUri, id);
                var node = factory.CreateUriNode(uri);

                return formatter.Format(node);
            }

            return sparql.Replace("@concepts", string.Join(" ", ids.Select(format)));
        }
    }
}
