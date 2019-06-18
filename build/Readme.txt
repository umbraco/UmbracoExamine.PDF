
  _    _ __  __ ____  _____           _____ ____  
 | |  | |  \/  |  _ \|  __ \    /\   / ____/ __ \ 
 | |  | | \  / | |_) | |__) |  /  \ | |   | |  | |
 | |  | | |\/| |  _ <|  _  /  / /\ \| |   | |  | |
 | |__| | |  | | |_) | | \ \ / ____ | |___| |__| |
  \____/|_|  |_|____/|_|  \_/_/    \_\_____\____/ 
                                                   
----------------------------------------------------

UmbracoExamine PDF indexer has been installed. 

The installer has added a new Indexer to Examine called PDFIndex.

**** NOTE: Not all PDFs can have text read from them! ****

*****************************************************************************
Umbraco V8
*****************************************************************************
New in Umbraco V8, Examine is setup in code rather than by config files.

To use the MultiSearcher in V8, you can instantiate it when needed rather than
configure it as was done in V7. 

var multiSearcher = new MultiIndexSearcher("MultiSearcher", new IIndex[] { 
    externalIndex, 
	pdfIndex 
});


*****************************************************************************
LICENSE
*****************************************************************************

The source code for UmbracoExamine.PDF is licensed under the MIT license. 