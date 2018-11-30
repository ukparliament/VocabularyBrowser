namespace WebApplication1
{
    using System.Collections.Generic;
    using VDS.RDF;
    using VDS.RDF.Dynamic;

    public class Concept : SkosResource
    {
        public Concept(INode node) : base(node) { }

        public ICollection<string> AltLabel => new DynamicObjectCollection<string>(this, "altLabel");

        public ICollection<string> Definition => new DynamicObjectCollection<string>(this, "definition");

        public ICollection<string> HistoryNote => new DynamicObjectCollection<string>(this, "historyNote");

        public ICollection<string> Notation => new DynamicObjectCollection<string>(this, "notation");

        public ICollection<ConceptScheme> InScheme => new DynamicObjectCollection<ConceptScheme>(this, "inScheme");

        public ICollection<Collection> Parent => new DynamicSubjectCollection<Collection>("member", this);

        public ICollection<Concept> Narrower => new DynamicObjectCollection<Concept>(this, "narrower");

        public ICollection<Concept> Broader => new DynamicObjectCollection<Concept>(this, "broader");
    }
}
