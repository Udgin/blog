using System.IO;

namespace blg
{
    internal interface IBlogConfiguration
    {
        string SourceFolder { get; }
        string TargetFolder { get; }
        string CardTemplatePath { get; }
        string ArticleTemplatePath { get; }
        string IndexTemplatePath { get; }
        string FolderImagePath { get; }
        string PrismJS { get; }
        string PrismCSS { get; }
        string TagTemplatePath { get; }
        string PathToMathJS { get; }
        string ArticlesFolder { get; }
    }
    internal class BlogConfiguration : IBlogConfiguration
    {
        private readonly string _currentFolder;

        public BlogConfiguration(string currentFolder)
        {
            _currentFolder = currentFolder;
        }
        public string SourceFolder { get { return _currentFolder; } }
        public string ArticlesFolder { get { return Path.Combine(_currentFolder, "articles"); } }
        public string TargetFolder { get; } = @"C:\Users\ea_pyl\projects\blog";
        public string CardTemplatePath { get { return Path.Combine(SourceFolder, @"theme\card.template.html"); } }
        public string ArticleTemplatePath { get { return Path.Combine(SourceFolder, @"theme\article.template.html"); } }
        public string IndexTemplatePath { get { return Path.Combine(SourceFolder, @"theme\index.template.html"); } }
        public string TagTemplatePath { get { return Path.Combine(SourceFolder, @"theme\tag.template.html"); } }
        public string FolderImagePath { get { return Path.Combine(SourceFolder, @"theme\folder.jpg"); } }
        public string PrismJS { get { return Path.Combine(SourceFolder, @"theme\prism.js"); } }
        public string PrismCSS { get { return Path.Combine(SourceFolder, @"theme\prism.css"); } }
        public string PathToMathJS { get; } = @"https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/MathJax.js?config=TeX-MML-AM_CHTML";
    }
}