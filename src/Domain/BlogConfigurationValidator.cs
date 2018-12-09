using FluentValidation;

namespace blg.Domain
{
    internal class BlogConfigurationValidator : AbstractValidator<BlogConfiguration>
    {
        public BlogConfigurationValidator()
        {
            RuleFor(x => x.SourceFolder).NotEmpty();
            RuleFor(x => x.TargetFolder).NotEmpty();
            RuleFor(x => x.ArticleTemplatePath).NotEmpty();
            RuleFor(x => x.ArticlesFolder).NotEmpty();
            RuleFor(x => x.CardTemplatePath).NotEmpty();
            RuleFor(x => x.Favicon).NotEmpty();
            RuleFor(x => x.FolderImagePath).NotEmpty();
            RuleFor(x => x.IndexTemplatePath).NotEmpty();
            RuleFor(x => x.PathToMathJS).NotEmpty();
            RuleFor(x => x.PrismCSS).NotEmpty();
            RuleFor(x => x.PrismJS).NotEmpty();
            RuleFor(x => x.TagTemplatePath).NotEmpty();
        }
    }
}