using System.IO;

namespace blg.Domain
{
    internal class BlogConfiguration : IBlogConfiguration
    {
        public string SourceFolder { get; set; }
        public string TargetFolder { get { return Path.Combine(SourceFolder, "blog"); } }
        public string ArticlesFolder { get { return Path.Combine(SourceFolder, "articles"); } }
        public string CardTemplatePath { get { return Path.Combine(SourceFolder, @"theme\card.template.html"); } }
        public string ArticleTemplatePath { get { return Path.Combine(SourceFolder, @"theme\article.template.html"); } }
        public string IndexTemplatePath { get { return Path.Combine(SourceFolder, @"theme\index.template.html"); } }
        public string TagTemplatePath { get { return Path.Combine(SourceFolder, @"theme\tag.template.html"); } }
        public string FuseTemplatePath { get { return Path.Combine(SourceFolder, @"theme\fuse.js"); } }
        public string FolderImagePath { get { return Path.Combine(SourceFolder, @"theme\folder.jpg"); } }
        public string PrismJS { get { return Path.Combine(SourceFolder, @"theme\prism.js"); } }
        public string PrismCSS { get { return Path.Combine(SourceFolder, @"theme\prism.css"); } }
        public string PathToMathJS { get; } = @"https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/MathJax.js?config=TeX-MML-AM_CHTML";
        public string Favicon { get { return Path.Combine(SourceFolder, @"theme\favicon.ico"); } }
        public string BlogTitle { get { return "Programming notes"; } }
    }
}