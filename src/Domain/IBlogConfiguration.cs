namespace blg.Domain
{
    internal interface IBlogConfiguration
    {
        string SourceFolder { get; set; }
        string TargetFolder { get; }
        string ArticlesFolder { get; }
        string CardTemplatePath { get; }
        string ArticleTemplatePath { get; }
        string IndexTemplatePath { get; }
        string TagTemplatePath { get; }
        string FuseTemplatePath { get; }
        string FolderImagePath { get; }
        string PrismJS { get; }
        string PrismCSS { get; }
        string PathToMathJS { get; }
        string Favicon { get; }
        string BlogTitle { get; }
    }
}