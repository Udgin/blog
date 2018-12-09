using FluentValidation;

namespace blg.Domain
{
    internal class CardEntityValidator : AbstractValidator<CardEntity>
    {
        public CardEntityValidator()
        {
            RuleFor(x => x.RelativePath).NotEmpty();
            RuleFor(x => x.ArticleTitle).SetValidator(new ArticleTitleValidator());
        }
    }
}