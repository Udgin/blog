using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using blg.Common;
using blg.Domain;
using Markdig;
using MediatR;
using FluentValidation;

namespace blg.Application
{
    internal class CreateArticlePageCommand : IRequest<CardEntity>, ILoggedRequest
    {
        public CreateArticlePageCommand(string relativePath, string sourceFolder, string pathToFolderWithIndexFile)
        {
            SourceFolder = sourceFolder;
            RelativePath = relativePath;
            PathToFolderWithIndexFile = pathToFolderWithIndexFile;
        }
        public string SourceFolder { get; }
        public string RelativePath { get; }
        public string PathToFolderWithIndexFile { get; }

        public string Trace() => $"{nameof(CreateArticlePageCommand)}: {SourceFolder}, {RelativePath}";
    }
    internal class CreateArticlePageCommandHandler : IRequestHandler<CreateArticlePageCommand, CardEntity>
    {
        private readonly IFileSystem _fileSystem;
        private readonly IMediator _mediator;
        private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        private readonly IValidator<CardEntity> _cardValidator;
        public CreateArticlePageCommandHandler(
            IFileSystem fileSystem,
            IMediator mediator,
            IValidator<CardEntity> cardValidator
            )
        {
            _fileSystem = fileSystem;
            _mediator = mediator;
            _cardValidator = cardValidator;
        }
        public async Task<CardEntity> Handle(CreateArticlePageCommand request, CancellationToken cancellationToken)
        {
            var configuration = await _mediator.Send(new GetConfigurationCommand(request.SourceFolder));

            var articleTemplate = await _mediator.Send(new GetTemplateCommand(configuration.ArticleTemplatePath));
            var fullPathToArticle = Path.Combine(configuration.ArticlesFolder, request.RelativePath);

            var linesOfMarkdownArticle = await _fileSystem.ReadAllLinesAsync(fullPathToArticle);

            var title = await _mediator.Send(new ParseTitleCommand(linesOfMarkdownArticle));

            var bodyOfArticle = linesOfMarkdownArticle.Skip(title.Size);

            var htmlContent = Markdown.ToHtml(string.Join(Environment.NewLine, bodyOfArticle), _pipeline);

            var contentOfArticle = articleTemplate
                .Replace("{{BODY}}", htmlContent)
                .Replace("{{TITLE}}", title.Title)
                .Replace("{{LINK}}", Utils.RelativePath(
                    request.PathToFolderWithIndexFile,
                    Path.Combine(configuration.TargetFolder, "index.html")))
                .Replace("{{LINK_FAVICON}}", Utils.RelativePath(
                    request.PathToFolderWithIndexFile,
                    Path.Combine(configuration.TargetFolder, "favicon.ico")));

            contentOfArticle = await _mediator.Send(new AddArticleStylesCommand(
                request.SourceFolder,
                contentOfArticle,
                htmlContent,
                request.RelativePath
            ));
            contentOfArticle = await _mediator.Send(new AddArticleScriptsCommand(
                request.SourceFolder,
                contentOfArticle,
                htmlContent,
                request.RelativePath,
                title.Script
            ));

            var linksInArticle = await _mediator.Send(new FindLinksCommand(bodyOfArticle));

            var fullPathToHtmlArticle =
                    Path.Combine(configuration.TargetFolder,
                        request.RelativePath.TrimStart('\\').TrimStart('/')).Replace(".md", ".html");

            foreach (var link in linksInArticle)
            {
                contentOfArticle = await _mediator.Send(new AddLinkToArticleCommand(
                    contentOfArticle,
                    fullPathToArticle,
                    link,
                    fullPathToHtmlArticle
                ));
            }

            if (title.Publish)
            {
                var uglified = await _mediator.Send(new UglifyCommand(contentOfArticle, TypeOfContent.Html));
                await _fileSystem.WriteAllTextAsync(fullPathToHtmlArticle, uglified);
            }

            var cardEntity = new CardEntity {
                ArticleTitle = title,
                RelativePath = Path.GetFileNameWithoutExtension(request.RelativePath) + ".html"
            };

            await _cardValidator.ValidateAndThrowAsync(cardEntity);

            return cardEntity;
        }
    }
}