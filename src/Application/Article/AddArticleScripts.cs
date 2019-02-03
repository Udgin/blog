using System.IO;
using System.Threading;
using System.Threading.Tasks;
using blg.Common;
using MediatR;

namespace blg.Application
{
    internal class AddArticleScriptsCommand : IRequest<string>
    {
        public AddArticleScriptsCommand(
            string sourceFolder,
            string markdownContent,
            string htmlContent,
            string relativePath,
            string customScript)
        {
            SourceFolder = sourceFolder;
            MarkdownContent = markdownContent;
            HtmlContent = htmlContent;
            RelativePath = relativePath;
            CustomScript = customScript;
        }

        public string SourceFolder { get; }
        public string MarkdownContent { get; }
        public string HtmlContent { get; }
        public string RelativePath { get; }
        public string CustomScript { get; }
    }

    internal class AddArticleScriptsHandler : IRequestHandler<AddArticleScriptsCommand, string>
    {
        private readonly IMediator _mediator;

        public AddArticleScriptsHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<string> Handle(AddArticleScriptsCommand request, CancellationToken cancellationToken)
        {
            var configuration = await _mediator.Send(new GetConfigurationCommand(request.SourceFolder));
            var pathToPrismJs = Path.Combine(configuration.TargetFolder, Path.GetFileName(configuration.PrismJS));
            var template = "<script async src=\"{0}\"></script>";
            var scripts = string.Empty;
            if (request.HtmlContent.Contains("<code"))
            {
                scripts += string.Format(template,
                    Utils.GetRelativePathToStaticResource(
                        configuration.TargetFolder,
                        request.RelativePath,
                        pathToPrismJs));
            }
            if (request.HtmlContent.Contains("<span class=\"math\""))
            {
                scripts += string.Format(template, configuration.PathToMathJS);
            }
            if (!string.IsNullOrWhiteSpace(request.CustomScript))
            {
                scripts += $"<script>{request.CustomScript}</script>";
            }
            return request.MarkdownContent.Replace("{{SCRIPTS}}", scripts);
        }
    }
}