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
    using System;
    using System.Collections.Generic;
    using VDS.RDF;
    using VDS.RDF.Dynamic;
    using VDS.RDF.Nodes;
    using VDS.RDF.Skos;

    public class SkosResource : DynamicNode
    {
        public string Id => Program.BaseUri.MakeRelativeUri((this as IUriNode).Uri).ToString();

        public SkosResource(INode node) : base(node, new Uri(SkosHelper.Namespace)) { }

        public ICollection<string> PrefLabel => new DynamicObjectCollection<string>(this, "prefLabel");
    }

    internal class a
    {
        private void b()
        {
            var a = new LangString("a", "b");
            c(a);
        }

        private void c(string x) { }
    }

    internal class LangString : IEquatable<LangString>
    {
        private readonly string s;
        private readonly string lang;

        public LangString(string s, string lang)
        {
            if (s is null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            if (string.IsNullOrWhiteSpace(lang))
            {
                throw new ArgumentException("", nameof(lang));
            }

            this.s = s;
            this.lang = lang;
        }

        public static implicit operator string(LangString s)
        {
            return s.ToString();
        }

        public static bool operator ==(LangString string1, LangString string2)
        {
            return EqualityComparer<LangString>.Default.Equals(string1, string2);
        }

        public static bool operator !=(LangString string1, LangString string2)
        {
            return !(string1 == string2);
        }

        public override string ToString()
        {
            return this.s;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as LangString);
        }

        public bool Equals(LangString other)
        {
            return other != null &&
                   this.s == other.s &&
                   this.lang == other.lang;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.s, this.lang);
        }
    }
}
