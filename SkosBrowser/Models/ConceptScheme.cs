namespace WebApplication1
{
    using System.Collections.Generic;
    using VDS.RDF;
    using VDS.RDF.Dynamic;

    public class ConceptScheme : SkosResource
    {
        public ConceptScheme(INode node) : base(node) { }

        public ICollection<Concept> HasTopConcept => new DynamicObjectCollection<Concept>(this, "hasTopConcept");
    }
}
