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
        public CreateArticlePageCommand(string relativePath, string sourceFolder)
        {
            SourceFolder = sourceFolder;
            RelativePath = relativePath;
        }
        public string SourceFolder { get; }
        public string RelativePath { get; }

        public string Trace() => $"{nameof(CreateArticlePageCommand)}: {SourceFolder}, {RelativePath}";
    }
    internal class CreateArticlePageCommandHandler : IRequestHandler<CreateArticlePageCommand, CardEntity>
    {
        private readonly IFileSystem _fileSystem;
        private readonly IMediator _mediator;
        private readonly MarkdownPipeline _pipeline;
        private readonly IValidator<CardEntity> _cardValidator;
        public CreateArticlePageCommandHandler(
            IFileSystem fileSystem,
            IMediator mediator,
            MarkdownPipeline pipeline,
            IValidator<CardEntity> cardValidator
            )
        {
            _fileSystem = fileSystem;
            _mediator = mediator;
            _pipeline = pipeline;
            _cardValidator = cardValidator;
        }
        public async Task<CardEntity> Handle(CreateArticlePageCommand request, CancellationToken cancellationToken)
        {
            var configuration = await _mediator.Send(new GetConfigurationCommand(request.SourceFolder));

            var articleTemplate = await _mediator.Send(new GetTemplateCommand(configuration.ArticleTemplatePath));
            var articleFolder = configuration.ArticlesFolder;
            var fullPathToArticle = Path.Combine(articleFolder, request.RelativePath);

            var linesOfMarkdownArticle = await _fileSystem.ReadAllLinesAsync(fullPathToArticle);

            var title = await _mediator.Send(new ParseTitleCommand(linesOfMarkdownArticle));

            var fullPathToHtmlArticle =
                    Path.Combine(configuration.TargetFolder,
                        request.RelativePath.TrimStart('\\').TrimStart('/')).Replace(".md", ".html");

            var bodyOfArticle = linesOfMarkdownArticle.Skip(title.Size);

            var htmlContent = Markdown.ToHtml(string.Join(Environment.NewLine, bodyOfArticle), _pipeline);

            var contentOfArticle = articleTemplate.Replace("{{BODY}}", htmlContent);

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

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullPathToHtmlArticle);

            contentOfArticle = contentOfArticle.Replace("{{TITLE}}", fileNameWithoutExtension);

            var linksInArticle = await _mediator.Send(new FindLinksCommand(bodyOfArticle));
            var folderForAssets = Path.GetDirectoryName(fullPathToHtmlArticle);

            foreach (var link in linksInArticle)
            {
                var fullPathToAsset = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fullPathToArticle), link));

                if (!_fileSystem.FileExists(fullPathToAsset)) continue;

                var targetPathForAsset = Path.Combine(folderForAssets, fileNameWithoutExtension, Path.GetFileName(fullPathToAsset));

                if (!_fileSystem.DirectoryExists(Path.GetDirectoryName(targetPathForAsset)))
                {
                    _fileSystem.DirectoryCreate(Path.GetDirectoryName(targetPathForAsset));
                }
                _fileSystem.CopyFile(fullPathToAsset, targetPathForAsset);
                contentOfArticle = contentOfArticle.Replace(link, Path.Combine(fileNameWithoutExtension, Path.GetFileName(fullPathToAsset)));
            }

            var linkWithImage =  linksInArticle.FirstOrDefault(x => x.EndsWith(".jpg") || x.EndsWith(".png"));

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