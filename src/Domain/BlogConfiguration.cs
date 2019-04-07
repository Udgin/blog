using System.IO;

namespace blg.Domain
{
    internal class BlogConfiguration : IBlogConfiguration
    {
        private const string ThemeFolder = "theme_bulma";
        public string SourceFolder { get; set; }
        public string TargetFolder { get { return Path.Combine(SourceFolder, "blog"); } }
        public string ArticlesFolder { get { return Path.Combine(SourceFolder, "articles"); } }
        public string CardTemplatePath { get { return Path.Combine(SourceFolder, ThemeFolder + @"\card.template.html"); } }
        public string ArticleTemplatePath { get { return Path.Combine(SourceFolder, ThemeFolder + @"\article.template.html"); } }
        public string IndexTemplatePath { get { return Path.Combine(SourceFolder, ThemeFolder + @"\index.template.html"); } }
        public string TagTemplatePath { get { return Path.Combine(SourceFolder, ThemeFolder + @"\tag.template.html"); } }
        public string FuseTemplatePath { get { return Path.Combine(SourceFolder, ThemeFolder + @"\fuse.js"); } }
        public string PrismJS { get { return Path.Combine(SourceFolder, ThemeFolder + @"\prism.js"); } }
        public string PrismCSS { get { return Path.Combine(SourceFolder, ThemeFolder + @"\prism.css"); } }
        public string PathToMathJS { get; } = @"https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/MathJax.js?config=TeX-MML-AM_CHTML";
        public string Favicon { get { return Path.Combine(SourceFolder, ThemeFolder + @"\favicon.ico"); } }
        public string BlogTitle { get { return "Programming notes"; } }
    }
}