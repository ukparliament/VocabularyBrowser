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
    using VDS.RDF;
    using VDS.RDF.Dynamic;

    public class Concept : SkosResource
    {
        public Concept(INode node) : base(node) { }

        public ICollection<string> AltLabel => new DynamicObjectCollection<string>(this, "altLabel");

        public ICollection<string> Definition => new DynamicObjectCollection<string>(this, "definition");

        public ICollection<string> EditorialNote => new DynamicObjectCollection<string>(this, "editorialNote");

        public ICollection<string> HistoryNote => new DynamicObjectCollection<string>(this, "historyNote");

        public ICollection<ConceptScheme> TopConceptOf => new DynamicObjectCollection<ConceptScheme>(this, "topConceptOf");

        public ICollection<string> ScopeNote => new DynamicObjectCollection<string>(this, "scopeNote");

        public ICollection<string> SesId => new DynamicObjectCollection<string>(this, "https://id.parliament.uk/schema/sesId");

        public ICollection<string> Notation => new DynamicObjectCollection<string>(this, "notation");

        public ICollection<ConceptScheme> InScheme => new DynamicObjectCollection<ConceptScheme>(this, "inScheme");

        public ICollection<Collection> Parent => new DynamicSubjectCollection<Collection>("member", this);

        public ICollection<Concept> Narrower => new DynamicObjectCollection<Concept>(this, "narrower");

        public ICollection<Concept> Broader => new DynamicObjectCollection<Concept>(this, "broader");

        public ICollection<Concept> Related => new DynamicObjectCollection<Concept>(this, "related");
    }
}
