using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NUglify;

namespace blg.Application
{
    internal class UglifyHtmlCommand : IRequest<string>, ILoggedRequest
    {
        public UglifyHtmlCommand(string content)
        {
            Content = content;
        }

        public string Content { get; }

        public string Trace() => $"{nameof(UglifyHtmlCommand)}";
    }

    internal class UglifyHtmlCommandHandler : IRequestHandler<UglifyHtmlCommand, string>
    {
        public Task<string> Handle(UglifyHtmlCommand request, CancellationToken cancellationToken)
        {
            var result = Uglify.Html(request.Content);
            if (result.HasErrors) return Task.FromResult(request.Content);
            return Task.FromResult(result.Code);
        }
    }
}