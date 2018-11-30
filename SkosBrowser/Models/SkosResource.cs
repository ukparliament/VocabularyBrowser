namespace WebApplication1
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
