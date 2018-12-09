using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace blg.Application
{
    internal class GetTemplateCommand : IRequest<string>, ILoggedRequest
    {
        public GetTemplateCommand(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public string Trace() => $"{nameof(GetTemplateCommand)}: {Path}";
    }

    internal class GetTemplateCommandHandler : IRequestHandler<GetTemplateCommand, string>
    {
        private readonly IFileSystem _fileSystem;

        public GetTemplateCommandHandler(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        public async Task<string> Handle(GetTemplateCommand request, CancellationToken cancellationToken) =>
            await _fileSystem.ReadAllTextAsync(request.Path);
    }
}