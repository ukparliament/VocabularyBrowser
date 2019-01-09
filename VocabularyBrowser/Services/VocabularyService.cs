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
    using Microsoft.Extensions.Configuration;
    using VDS.RDF;
    using VDS.RDF.Query;
    using VDS.RDF.Storage;

    public class VocabularyService : IDisposable
    {
        private readonly SparqlConnector connector;

        public VocabularyService(IConfiguration config)
        {
            this.connector = new SparqlConnector(new Uri(config.GetSection("VocabularyService")["SparqlEndpoint"]));
        }

        internal IGraph Execute(string sparql)
        {
            return connector.Query(sparql) as IGraph;
        }

        internal IGraph Execute(string sparql, Uri u)
        {
            var pp = new SparqlParameterizedString(sparql);

            if (!(u is null))
            {
                pp.SetUri("parameter", u);
            }

            return this.Execute(pp);
        }

        internal IGraph Execute(string sparql, string p)
        {

            var pp = new SparqlParameterizedString(sparql);

            if (!(p is null))
            {
                pp.SetLiteral("parameter", p);
            }

            return this.Execute(pp);
        }

        internal IGraph Execute(SparqlParameterizedString pp)
        {
            return this.Execute(pp.ToString());
        }

        void IDisposable.Dispose()
        {
            this.connector.Dispose();
        }
    }
}
