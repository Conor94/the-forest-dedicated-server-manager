using System;
using System.Windows.Documents;

namespace TheForestDSM.Factories
{
    public static class HyperlinkFactory
    {
        public static Hyperlink Create(string uri, string text)
        {
            return new Hyperlink(new Run(text))
            {
                NavigateUri = new Uri(uri)
            };
        }

        public static Hyperlink Create(string uri)
        {
            return Create(uri, uri);
        }
    }
}
