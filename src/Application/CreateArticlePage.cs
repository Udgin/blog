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

namespace blg.Application
{
    internal class CreateArticlePageCommand : IRequest<CardEntity>
    {
        public CreateArticlePageCommand(string relativePath, string sourceFolder, string targetFolder)
        {
            SourceFolder = sourceFolder;
            RelativePath = relativePath;
            TargetFolder = targetFolder;
        }
        public string SourceFolder { get; }
        public string RelativePath { get; }
        public string TargetFolder { get; }
    }
    internal class CreateArticlePageCommandHandler : IRequestHandler<CreateArticlePageCommand, CardEntity>
    {
        public CreateArticlePageCommandHandler(IFileSystem fileSystem, IMediator mediator, MarkdownPipeline pipeline)
        {
            _fileSystem = fileSystem;
            _mediator = mediator;
            _pipeline = pipeline;
        }
        public async Task<CardEntity> Handle(CreateArticlePageCommand request, CancellationToken cancellationToken)
        {
            var configuration = await _mediator.Send(new GetConfigurationCommand(request.SourceFolder));

            var articleTemplate = await _mediator.Send(new GetTemplateCommand(configuration.ArticleTemplatePath));
            var articleFolder = configuration.ArticlesFolder;
            var fullPathToArticle = Path.Combine(articleFolder, request.RelativePath);

            var linesOfMarkdownArticle = await _fileSystem.ReadAllLinesAsync(fullPathToArticle);

            var parsedTitle = ParseTitle(linesOfMarkdownArticle);

            var fullPathToHtmlArticle =
                    Path.Combine(request.TargetFolder,
                        request.RelativePath.TrimStart('\\').TrimStart('/')).Replace(".md", ".html");

            var bodyOfArticle = linesOfMarkdownArticle.Skip(parsedTitle.Item2);

            var htmlContent = Markdown.ToHtml(string.Join(Environment.NewLine, bodyOfArticle), _pipeline);

            var contentOfArticle = articleTemplate.Replace("{{BODY}}", htmlContent);

            contentOfArticle = AddStyles(contentOfArticle, htmlContent, request.RelativePath);
            contentOfArticle = AddScripts(contentOfArticle, htmlContent, request.RelativePath);

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullPathToHtmlArticle);

            contentOfArticle = contentOfArticle.Replace("{{TITLE}}", fileNameWithoutExtension);

            var linksInArticle = FindLinks(bodyOfArticle);
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

            await _fileSystem.WriteAllTextAsync(fullPathToHtmlArticle, contentOfArticle);

            return new CardEntity {
                Tags = parsedTitle.Item1,
                RelativePath = Path.GetFileNameWithoutExtension(request.RelativePath) + ".html"
            };

            string AddScripts(string content, string html, string relativePath)
            {
                var pathToPrismJs = Path.Combine(request.TargetFolder, Path.GetFileName(configuration.PrismJS));
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
                return content.Replace("{{SCRIPTS}}", scripts);
            }

            string AddStyles(string content, string html, string relativePath)
            {
                var pathToPrismCss = Path.Combine(request.TargetFolder, Path.GetFileName(configuration.PrismCSS));
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
                        request.TargetFolder :
                        Path.Combine(request.TargetFolder, Path.GetDirectoryName(relativePath)),
                    pathToResource
                );
        }

        private readonly Regex linkRegex
            = new Regex(@"\[.+\]\((?<url>.+)\)", RegexOptions.Compiled);

        private IEnumerable<string> FindLinks(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                foreach (Match match in linkRegex.Matches(line))
                {
                    for (int i = 0; i < match.Groups.Count; i++)
                    {
                        Group group = match.Groups[i];
                        if (group.Success && group.Name == "url")
                            yield return group.Value;
                    }
                }
            }
        }

        private const string MetaDataStart = "---";
        private readonly IFileSystem _fileSystem;
        private readonly IMediator _mediator;
        private readonly MarkdownPipeline _pipeline;
        private string[] _headers = new[] { "Title:", "Date:", "Tags:"};
        private (IDictionary<string, string>, int) ParseTitle(string[] lines)
        {
            var tags = new Dictionary<string, string>();

            if (lines[0] != MetaDataStart) return (tags, 0);
            string line;
            var i = 1;
            while ((line = lines[i++]) != MetaDataStart)
            {
                foreach (var head in _headers)
                {
                    if (line.StartsWith(head) && !string.IsNullOrWhiteSpace(line.Substring(head.Length)))
                    {
                        tags[head.TrimEnd(':')] = line.Substring(head.Length).Trim();
                    }
                }
            }
            return (tags, i);
        }
    }
}