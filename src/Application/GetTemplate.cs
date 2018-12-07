using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace blg.Application
{
    internal class GetTemplateCommand : IRequest<string>
    {
        public GetTemplateCommand(string path)
        {
            Path = path;
        }

        public string Path { get; }
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