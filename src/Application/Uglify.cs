using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NUglify;

namespace blg.Application
{
    internal enum TypeOfContent
    {
        Html,
        Js,
        Css
    }
    internal class UglifyCommand : IRequest<string>, ILoggedRequest
    {
        public UglifyCommand(string content, TypeOfContent type)
        {
            Content = content;
            Type = type;
        }

        public string Content { get; }
        public TypeOfContent Type { get; }

        public string Trace() => $"{nameof(UglifyCommand)}";
    }

    internal class UglifyCommandHandler : IRequestHandler<UglifyCommand, string>
    {
        public Task<string> Handle(UglifyCommand request, CancellationToken cancellationToken)
        {
            var result = GetUgligied();
            if (result.HasErrors) return Task.FromResult(request.Content);
            return Task.FromResult(result.Code);

            UglifyResult GetUgligied()
            {
                if (request.Type == TypeOfContent.Html) return Uglify.Html(request.Content);
                if (request.Type == TypeOfContent.Css) return Uglify.Css(request.Content);
                if (request.Type == TypeOfContent.Js) return Uglify.Js(request.Content);
                throw new ArgumentException(nameof(request.Type));
            }
        }
    }
}