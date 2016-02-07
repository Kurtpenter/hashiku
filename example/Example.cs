using System;
using System.Collections.Generic;
using net.curtoni.hashiku;

namespace example
{
    class Example
    {
        public static void Main(string[] args)
        {
            Random rnd = new Random();

            string template = "[Adjective] [noun] [verb]s [noun].\n    [Noun] [verb]s [noun].\n        [Noun] [verb]s.";

            var providers = new Dictionary<string, Hashiku.DataProvider>() {
                {
                    "adjective",
                    new Hashiku.ResourceDataProvider("net.curtoni.hashiku.adjectives.txt")
                },
                {
                    "noun",
                    new Hashiku.ResourceDataProvider("net.curtoni.hashiku.nouns.txt")
                },
                {
                    "verb",
                    new Hashiku.ResourceDataProvider("net.curtoni.hashiku.verbs.txt")
                }
            };

            for (int i = 0; i < 10; i++) {
                // SHA-1 160bit
                var buffer = new byte[160 / 8];
                rnd.NextBytes(buffer);
                Hashiku hashiku = new Hashiku(buffer, template, providers);

                Console.WriteLine(
                    string.Format("Hashiku for {0} is:\n{1}", BitConverter.ToString(buffer), hashiku)
                );
                Console.WriteLine();
            }
        }
    }
}
