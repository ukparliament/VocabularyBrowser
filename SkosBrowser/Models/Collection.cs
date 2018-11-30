namespace WebApplication1
{
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Dynamic;
    using VDS.RDF.Skos;

    public class Collection : SkosResource
    {
        public Collection(INode node) : base(node) { }

        public IEnumerable<Collection> Child => ((IEnumerable<Collection>)new DynamicObjectCollection<Collection>(this, "member")).Where(x => x.Graph.ContainsTriple(new Triple(x, x.Graph.CreateUriNode("rdf:type"), x.Graph.CreateUriNode(UriFactory.Create(SkosHelper.Collection)))));

        public IEnumerable<Concept> Member => ((IEnumerable<Concept>)new DynamicObjectCollection<Concept>(this, "member")).Where(x => x.Graph.ContainsTriple(new Triple(x, x.Graph.CreateUriNode("rdf:type"), x.Graph.CreateUriNode(UriFactory.Create(SkosHelper.Concept)))));

        public ICollection<Collection> Parent => new DynamicSubjectCollection<Collection>("member", this);
    }
}
