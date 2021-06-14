# UmbracoExamine.PDF

PDF indexing support in UmbracoExamine using [PdfPig](https://github.com/UglyToad/PdfPig)

## Installation

Install with Nuget:

    dotnet add package UmbracoExamine.PDF

You will then have a new Examine index called "PDFIndex" available.

## Multi index searchers

To use the MultiSearcher in V9, you can instantiate it when needed like:

```cs
using(var multiSearcher = new MultiIndexSearcher("MultiSearcher", new IIndex[] {
    externalIndex,
	pdfIndex
}))
{
 ...
};
```

Or you can register a multi-index searcher with the ExamineManager on startup like:

```cs
public class Startup
{
   ...

   public void ConfigureServices(IServiceCollection services)
   {
       ...
       services.AddExamineLuceneMultiSearcher("MultiSearcher", new string [] {
          Constants.UmbracoIndexes.ExternalIndexName,
          PdfIndexConstants.PdfIndexName
       });
   }

   ...
}
```

With this approach, the multi searcher will show up in the Examine dashboard and it can be resolved from the ExamineManager like:

```cs
if (_examineManager.TryGetSearcher("MultiSearcher", out var searcher))
{
    //TODO: use the `searcher` to search
}
```
## TestSite

Uses SqlCE and the db is part of git.

Username: me@mail.com
Password: 1234567890


## License

The source code for UmbracoExamine.PDF is licensed under the MIT license.
