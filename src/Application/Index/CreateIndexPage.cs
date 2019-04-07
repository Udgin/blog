using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using blg.Common;
using blg.Domain;
using MediatR;
using FluentValidation;
using System.Text;

namespace blg.Application
{
    internal class CreateIndexPageCommand : IRequest<CardEntity>, ILoggedRequest
    {
        public CreateIndexPageCommand(
            string sourceFolder,
            IList<CardEntity> cardEntities,
            string pathToIndexFolder,
            string title)
        {
            SourceFolder = sourceFolder;
            CardEntities = cardEntities;
            PathToIndexFolder = pathToIndexFolder;
            Title = title;
        }

        public string SourceFolder { get; }
        public IList<CardEntity> CardEntities { get; }
        public string PathToIndexFolder { get; }
        public string Title { get; }

        public string Trace() => $"{nameof(CreateIndexPageCommand)}: {Title}, {SourceFolder}, {CardEntities.Count}, {PathToIndexFolder}";
    }
    internal class CreateIndexPageCommandHandler : IRequestHandler<CreateIndexPageCommand, CardEntity>
    {
        private const int MaxCountOfTags = 7;
        private readonly IFileSystem _fileSystem;
        private readonly IMediator _mediator;
        private readonly IValidator<CardEntity> _cardValidator;

        public CreateIndexPageCommandHandler(IFileSystem fileSystem,
            IMediator mediator, IValidator<CardEntity> cardValidator)
        {
            _fileSystem = fileSystem;
            _mediator = mediator;
            _cardValidator = cardValidator;
        }
        public async Task<CardEntity> Handle(CreateIndexPageCommand request, CancellationToken cancellationToken)
        {
            var configuration = await _mediator.Send(new GetConfigurationCommand(request.SourceFolder));
            var cardTemplate = await _mediator.Send(new GetTemplateCommand(configuration.CardTemplatePath));
            var tagTemplate = await _mediator.Send(new GetTemplateCommand(configuration.TagTemplatePath));
            var indexTemplate = await _mediator.Send(new GetTemplateCommand(configuration.IndexTemplatePath));

            var fullPathForIndexPage = request.PathToIndexFolder;
            var htmlFilePath = Path.Combine(fullPathForIndexPage, "index.html");

            var htmlCards = new List<string>();
            var tags = new Dictionary<string, int>();
            foreach (var childCards in request.CardEntities
                .Where(x => x.ArticleTitle.Publish)
                .OrderByDescending(x => x.ArticleTitle.Date))
            {
                htmlCards.Add(cardTemplate
                    .Replace("{{TITLE}}", childCards.ArticleTitle.Title)
                    .Replace("{{DATE}}", childCards.SortDate)
                    .Replace("{{SUBTITLE}}", string.Join(", ", childCards.ArticleTitle.Tags))
                    .Replace("{{LINK}}", childCards.RelativePath));
            }

            var tagsHtml = new StringBuilder();

            var mostPopularTags = request.CardEntities
                .Where(x => x.ArticleTitle.Publish)
                .SelectMany(x => x.ArticleTitle.Tags)
                .GroupBy(x => x)
                .OrderByDescending(x => x.Count())
                .Take(MaxCountOfTags)
                .ToDictionary(x => x.Key, x => x.Count());

            foreach (var pair in mostPopularTags)
            {
                tagsHtml.Append(tagTemplate
                    .Replace("{{COUNT}}", pair.Value.ToString())
                    .Replace("{{NAME}}", pair.Key)
                );
            }

            var contentOfIndex = indexTemplate
                .Replace("{{BODY}}", string.Join(string.Empty, htmlCards))
                .Replace("{{TITLE}}", request.Title)
                .Replace("{{TAGS}}", tagsHtml.ToString())
                .Replace("{{LINK}}", Utils.RelativePath(
                    request.PathToIndexFolder,
                    configuration.TargetFolder));

            var uglified = await _mediator.Send(new UglifyCommand(contentOfIndex, TypeOfContent.Html));

            await _fileSystem.WriteAllTextAsync(htmlFilePath, uglified);

            var card = new CardEntity {
                ArticleTitle = new ArticleTitle {
                    Title = request.Title,
                    Date = DateTime.MaxValue
                },
                RelativePath = request.Title + "/index.html"
            };

            await _cardValidator.ValidateAndThrowAsync(card, "index");

            await _mediator.Send(new FuseSearchCommand(
                request.CardEntities.Select(x => x.ArticleTitle),
                fullPathForIndexPage,
                request.SourceFolder)
            );

            return card;
        }
    }
}