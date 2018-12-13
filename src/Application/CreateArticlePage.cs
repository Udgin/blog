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

            contentOfArticle = AddStyles(contentOfArticle, htmlContent, request.RelativePath);
            contentOfArticle = AddScripts(contentOfArticle, htmlContent, title.Script, request.RelativePath);

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
                await _fileSystem.WriteAllTextAsync(fullPathToHtmlArticle, contentOfArticle);
            }

            var cardEntity = new CardEntity {
                ArticleTitle = title,
                RelativePath = Path.GetFileNameWithoutExtension(request.RelativePath) + ".html"
            };

            await _cardValidator.ValidateAndThrowAsync(cardEntity);

            return cardEntity;

            string AddScripts(string content, string html, string customScript, string relativePath)
            {
                var pathToPrismJs = Path.Combine(configuration.TargetFolder, Path.GetFileName(configuration.PrismJS));
                var template = "<script async src=\"{0}\"></script>";
                var scripts = string.Empty;
                if (html.Contains("<code"))
                {
                    scripts += string.Format(template, GetRelativePathToStaticResource(relativePath, pathToPrismJs));
                }
                if (html.Contains("<span class=\"math\""))
                {
                    scripts += string.Format(template, configuration.PathToMathJS);
                }
                if (!string.IsNullOrWhiteSpace(customScript))
                {
                    scripts += $"<scripts>{customScript}</script>";
                }
                return content.Replace("{{SCRIPTS}}", scripts);
            }

            string AddStyles(string content, string html, string relativePath)
            {
                var pathToPrismCss = Path.Combine(configuration.TargetFolder, Path.GetFileName(configuration.PrismCSS));
                var template = "<link rel=\"stylesheet\" href=\"{0}\"></link>";
                var cssStyles = string.Empty;
                if (html.Contains("<code"))
                {
                    cssStyles += string.Format(template, GetRelativePathToStaticResource(relativePath, pathToPrismCss));
                }
                return content.Replace("{{STYLES}}", cssStyles);
            }

            string GetRelativePathToStaticResource(string relativePath, string pathToResource) =>
                Utils.RelativePath (
                    Path.GetDirectoryName(relativePath) == null ?
                        configuration.TargetFolder :
                        Path.Combine(configuration.TargetFolder, Path.GetDirectoryName(relativePath)),
                    pathToResource
                );
        }
    }
}