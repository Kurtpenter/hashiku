# hashiku

```
Worker bees can leave
  Even drones can fly away
    The queen is their slave

— Fight Club, 1999
```

Hashiku should help to remember long *hash* codes such as a SHA-1 by translating them into sentences.
If you are lucky they may even sound like a Haiku (https://en.wikipedia.org/wiki/Haiku). :smirk:

You can define a sentence template such as:
```csharp
string template = "[Adjective] [noun] [verb]s [noun].\n    [Noun] [verb]s [noun].\n        [Noun] [verb]s.";
```

Then associate each token to a dataprovider:

```csharp
var providers = new Dictionary<string, Hashiku.DataProvider>() {
  { "adjective", new Hashiku.ResourceDataProvider("net.curtoni.hashiku.adjectives.txt")},
  { "noun", new Hashiku.ResourceDataProvider("net.curtoni.hashiku.nouns.txt")},
  { "verb", new Hashiku.ResourceDataProvider("net.curtoni.hashiku.verbs.txt")}
};
```

If you capitalize the first letter of the token, the resulting word will be capitalized.

Then make a hashiku:

```csharp
byte[] buffer = ...

Hashiku hashiku = new Hashiku(buffer, template, providers);
```

And this is an example output:
```
Hashiku for 75-79-30-89-78-DF-C8-DD-D1-C6-9F-58-BE-1E-D7-88-04-44-CE-F7 is:
Stereotyped company shades afterthought.
    Playground streamlines division.
        Animal talks.
```
