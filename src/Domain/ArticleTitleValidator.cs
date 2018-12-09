using FluentValidation;

namespace blg.Domain
{
    internal class ArticleTitleValidator : AbstractValidator<ArticleTitle>
    {
        public ArticleTitleValidator()
        {
            RuleSet("index", () =>
            {
                RuleFor(x => x.Title).NotEmpty();
            });
            RuleFor(x => x.Date).NotEmpty();
            RuleFor(x => x.Size).GreaterThan(2);
        }
    }
}