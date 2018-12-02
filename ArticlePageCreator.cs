using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Markdig;

namespace blg
{
    internal interface IArticlePageCreator
    {
        Task<CardEntity> Create(string relativePath);
    }
    internal class ArticlePageCreator : IArticlePageCreator
    {
        private readonly MarkdownPipeline _pipeline;
        private readonly IBlogConfiguration _configuration;
        private readonly IFileSystem _fileSystem;
        private readonly Lazy<string> _template;

        public ArticlePageCreator(
            IFileSystem fileSystem,
            IBlogConfiguration configuration,
            MarkdownPipeline pipeline
        )
        {
            _pipeline = pipeline;
            _configuration = configuration;
            _fileSystem = fileSystem;
            _template = new Lazy<string>(() => _fileSystem.ReadAllText(_configuration.ArticleTemplatePath));
        }
        private string PathToPrismCSS =>
            Path.Combine(_configuration.TargetFolder, Path.GetFileName(_configuration.PrismCSS));
        private string PathToPrismJS =>
            Path.Combine(_configuration.TargetFolder, Path.GetFileName(_configuration.PrismJS));
        public async Task<CardEntity> Create(string relativePath)
        {
            var fullPathToArticle = Path.Combine(_configuration.ArticlesFolder, relativePath);

            var linesOfMarkdownArticle = await _fileSystem.ReadAllLinesAsync(fullPathToArticle);

            var parsedTitle = ParseTitle(linesOfMarkdownArticle);

            var fullPathToHtmlArticle =
                    Path.Combine(_configuration.TargetFolder,
                        relativePath.TrimStart('\\').TrimStart('/')).Replace(".md", ".html");

            var bodyOfArticle = linesOfMarkdownArticle.Skip(parsedTitle.Item2);

            var htmlContent = Markdown.ToHtml(string.Join(Environment.NewLine, bodyOfArticle), _pipeline);

            var contentOfArticle = _template.Value.Replace("{{BODY}}", htmlContent);

            contentOfArticle = AddStyles(contentOfArticle, htmlContent, relativePath);
            contentOfArticle = AddScripts(contentOfArticle, htmlContent, relativePath);

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
                RelativePath = Path.GetFileNameWithoutExtension(relativePath) + ".html",
                ImageRelativePath = linkWithImage != null ?
                    Path.Combine(fileNameWithoutExtension, Path.GetFileName(linkWithImage)) :
                    string.Empty
            };
        }

        private string GetRelativePathToStaticResource(string relativePath, string pathToResource) =>
            Utils.RelativePath (
                Path.GetDirectoryName(relativePath) == null ?
                    _configuration.TargetFolder :
                    Path.Combine(_configuration.TargetFolder, Path.GetDirectoryName(relativePath)),
                pathToResource
            );

        private string AddScripts(string content, string html, string relativePath)
        {
            var template = "<script async src=\"{0}\"></script>";
            var scripts = string.Empty;
            if (html.Contains("<code"))
            {
                scripts += string.Format(template, GetRelativePathToStaticResource(relativePath, PathToPrismJS));
            }
            if (html.Contains("<span class=\"math\""))
            {
                scripts += string.Format(template, _configuration.PathToMathJS);
            }
            return content.Replace("{{SCRIPTS}}", scripts);
        }

        private string AddStyles(string content, string html, string relativePath)
        {
            var template = "<link rel=\"stylesheet\" href=\"{0}\"></link>";
            var cssStyles = string.Empty;
            if (html.Contains("<code"))
            {
                cssStyles += string.Format(template, GetRelativePathToStaticResource(relativePath, PathToPrismCSS));
            }
            return content.Replace("{{STYLES}}", cssStyles);
        }

        private const string MetaDataStart = "---";
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
    }
}