using System.IO;
using System.Threading;
using System.Threading.Tasks;
using blg.Common;
using MediatR;

namespace blg.Application
{
    internal class AddArticleStylesCommand : IRequest<string>
    {
        public AddArticleStylesCommand(
            string sourceFolder,
            string markdownContent,
            string htmlContent,
            string relativePath)
        {
            SourceFolder = sourceFolder;
            MarkdownContent = markdownContent;
            HtmlContent = htmlContent;
            RelativePath = relativePath;
        }

        public string SourceFolder { get; }
        public string MarkdownContent { get; }
        public string HtmlContent { get; }
        public string RelativePath { get; }
    }

    internal class AddArticleStylesHandler : IRequestHandler<AddArticleStylesCommand, string>
    {
        private readonly IMediator _mediator;

        public AddArticleStylesHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<string> Handle(AddArticleStylesCommand request, CancellationToken cancellationToken)
        {
            var configuration = await _mediator.Send(new GetConfigurationCommand(request.SourceFolder));
            var pathToPrismCss = Path.Combine(configuration.TargetFolder, Path.GetFileName(configuration.PrismCSS));
            var template = "<link rel=\"stylesheet\" href=\"{0}\"></link>";
            var cssStyles = string.Empty;
            if (request.HtmlContent.Contains("<code"))
            {
                cssStyles += string.Format(
                    template,
                    Utils.GetRelativePathToStaticResource(configuration.TargetFolder, request.RelativePath, pathToPrismCss));
            }
            return request.MarkdownContent.Replace("{{STYLES}}", cssStyles);
        }
    }
}