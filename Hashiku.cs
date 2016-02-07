using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;

namespace net.curtoni.hashiku
{
    public class Hashiku
    {
        private BitArray bits;
        private Dictionary<string, DataProvider> dataProviders = new Dictionary<string, DataProvider>();
        private string template;

        public Hashiku(byte[] hash, string template, Dictionary<string, DataProvider> dataProviders)
        {
            this.bits = new BitArray(hash);
            this.dataProviders = dataProviders;
            this.template = template;
        }

        public override string ToString()
        {
            string hashiku = template;
            int tokens = 0;
            foreach (string key in dataProviders.Keys)
            {
                string token = "[" + key + "]";
                int count = hashiku.Select((c, i) => hashiku.Substring(i)).Count(sub => sub.StartsWith(token, StringComparison.CurrentCultureIgnoreCase));
                tokens += count;
            }

            int paddedSize = (int) (Math.Ceiling((double)bits.Length / tokens) * tokens);
            bool[] paddedBits = new bool[paddedSize];

            bits.CopyTo(paddedBits, 0);

            bool[] block = new bool[paddedSize / tokens];
            byte[] indexBytes = new byte[(int)Math.Ceiling((double)block.Length / 8) + 1];

            int j = 0;
            foreach (string key in dataProviders.Keys)
            {
                string token = "[" + key + "]";
                int count = hashiku.Select((c, i) => hashiku.Substring(i)).Count(sub => sub.StartsWith(token, StringComparison.CurrentCultureIgnoreCase));

                for (int i = 0; i < count; i++)
                {
                    int blockIndex = j++ * block.Length;
                    Array.ConstrainedCopy(paddedBits, blockIndex, block, 0, block.Length);
                    BitArray bitArray = new BitArray(block);
                    bitArray.CopyTo(indexBytes, 0);
                    BigInteger index = new BigInteger(indexBytes);
                    string value = dataProviders[key].GetValue(index);

                    hashiku = ReplaceFirst(hashiku, token, value);
                }
            }
            return hashiku;
        }

        string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search, StringComparison.CurrentCultureIgnoreCase);
            if (pos < 0)
            {
                return text;
            }
            if (char.IsUpper(text, pos + 1)) {
                replace = char.ToUpper(replace[0]) + replace.Substring(1);
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private static string cap(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public abstract class DataProvider {
            public abstract string GetValue(BigInteger index);
        }

        public class ArrayDataProvider : DataProvider {
            private string[] data;
            public ArrayDataProvider(string[] data) {
                this.data = data;
            }

            public override string GetValue(BigInteger index)
            {
                BigInteger remainder;
                BigInteger.DivRem(index, data.Length, out remainder);
                return data[(int) remainder];
            }
        }

        public class FileDataProvider : Hashiku.DataProvider {
            private readonly string fileName;
            private readonly int size;

            public FileDataProvider(string fileName) {
                this.fileName = fileName;
                size = File.ReadLines(fileName).Count();
            }

            #region implemented abstract members of DataProvider
            public override string GetValue(BigInteger index)
            {
                BigInteger remainder;
                BigInteger.DivRem(index, size, out remainder);
                return File.ReadLines(fileName).Skip((int) remainder).Take(1).First();
            }
            #endregion
        }

        public class ResourceDataProvider : Hashiku.DataProvider {
            private readonly int size;
            private readonly List<string> lines;

            public ResourceDataProvider(string resourceName) {
                lines = ReadLines(() => Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(resourceName),
                    Encoding.UTF8)
                    .ToList();

                size = lines.Count();
            }

            private IEnumerable<string> ReadLines(Func<Stream> streamProvider, Encoding encoding)
            {
                using (var stream = streamProvider())
                using (var reader = new StreamReader(stream, encoding))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        yield return line;
                    }
                }
            }

            #region implemented abstract members of DataProvider
            public override string GetValue(BigInteger index)
            {
                BigInteger remainder;
                BigInteger.DivRem(index, size, out remainder);
                return lines.Skip((int) remainder).Take(1).First();
            }
            #endregion
        }
    }
}

