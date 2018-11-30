namespace WebApplication1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Dynamic;
    using VDS.RDF.Skos;

    public class Skos : DynamicGraph
    {
        public Skos(IGraph graph) : base(graph) { }

        public ICollection<ConceptScheme> ConceptSchemes => new DynamicSubjectCollection<ConceptScheme>("rdf:type", this[SkosHelper.ConceptScheme] as DynamicNode);

        public ICollection<Collection> Collections => new DynamicSubjectCollection<Collection>("rdf:type", this[SkosHelper.Collection] as DynamicNode);

        public ICollection<Concept> Concepts => new DynamicSubjectCollection<Concept>("rdf:type", this[SkosHelper.Concept] as DynamicNode);
    }
}
