# UmbracoExamine.PDF

PDF indexing support in UmbracoExamine using [pdfSharp](https://github.com/empira/PDFsharp)

## Installation

Install with Nuget:

    Install-Package UmbracoCms.UmbracoExamine.PDF
  
You will then have a new Examine index called "PDFIndex" available.

## Multi index searchers

To use the MultiSearcher in V8, you can instantiate it when needed like:

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
[ComposeAfter(typeof(ExaminePdfComposer))] //this must execute after the ExaminePdfComposer composer
public class MyComposer : ComponentComposer<MyComponent>, IUserComposer
{
}

public class MyComponent : IComponent
{
    private readonly IExamineManager _examineManager;

    public MyComponent(IExamineManager examineManager)
    {
        _examineManager = examineManager;
    }

    public void Initialize()
    {
        //Get both the external and pdf index
        if (_examineManager.TryGetIndex(Constants.UmbracoIndexes.ExternalIndexName, out var externalIndex)
            && _examineManager.TryGetIndex(PdfIndexCreator.PdfIndexName, out var pdfIndex))
        {
            //register a multi searcher for both of them
            var multiSearcher = new MultiIndexSearcher("MultiSearcher", new IIndex[] { externalIndex, pdfIndex });
            _examineManager.AddSearcher(multiSearcher);
        }
    }

    public void Terminate() { }
}
```

With this approach, the multi searcher will show up in the Examine dashboard and it can be resolved from the ExamineManager like:

```cs
if (_examineManager.TryGetSearcher("MultiSearcher", out var searcher))
{
    //TODO: use the `searcher` to search
}
```

## Known issues

The implementation of `IPdfTextExtractor` in this library is using PDFSharp and some custom logic and does not deal with Unicode text and therefore some PDF texts cannot be read and will result in 'junk' strings
being read. It is certainly possible to replace the `IPdfTextExtractor` using your own composer like

```cs
composition.RegisterUnique<IPdfTextExtractor, MyCustomSharpTextExtractor>();
```

The iTextSharp library deals with Unicode in a better way but is a paid for license. If you wish to use iTextSharp or another PDF library you can easily swap out the
`IPdfTextExtractor` with your own implementation.

## License

The source code for UmbracoExamine.PDF is licensed under the MIT license. 
