# Eugene Pyl's blog/set of notes about programming
Tags:
.NET, Xamarin, IIS, JQuery, JavaScript, C#, CSharp, PowerShell, Linux, Git, Team City, Highchart, WinRT, Bootstrap, ASP.NET MVC,
MSSQL, Database, TFS, LINQ, Fiddle, API Facebook, TCP, Sniffer, StyleCop, WCF

Based on markdown file: blog.md. Every article should be separated by next line:
`---`

Every title of article should be started by '##' and ended with '- dd mmm, yyyy', e.g. - 14 November, 2016.

You should use gulp command (gulp default) to parse mentioned file, convert each created file to html, create index file using titles of articles and indexTemplate.html, create html page in 'article' folder for every article using articleTemplate.html.

Features:
- Added Disqus to every article;
- Automatically create index.html using the list of articles;
- Automatically sorting articles by date in the end of title of article.
- MathJax(mathematical formulas) is supported;
- Highlight for code is supported;

Used tools:
- Visual Studio Code;
- Disqus platform;
- Gulp;
- MathJax.js;
- Highlight.js;
- Markdown parser - marked;

