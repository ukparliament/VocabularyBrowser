namespace WebApplication1
{
    using System;
    using System.Collections.Generic;
    using VDS.RDF;
    using VDS.RDF.Dynamic;
    using VDS.RDF.Skos;

    public class Skos : DynamicGraph
    {
        public Skos(IGraph graph, Uri subjectBaseUri = null) : base(graph, subjectBaseUri) { }

        public ICollection<ConceptScheme> ConceptSchemes => new DynamicSubjectCollection<ConceptScheme>("rdf:type", this[SkosHelper.ConceptScheme] as DynamicNode);

        public ICollection<Collection> Collections => new DynamicSubjectCollection<Collection>("rdf:type", this[SkosHelper.Collection] as DynamicNode);

        public ICollection<Concept> Concepts => new DynamicSubjectCollection<Concept>("rdf:type", this[SkosHelper.Concept] as DynamicNode);
    }
}
