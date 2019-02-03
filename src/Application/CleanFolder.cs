using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace blg.Application
{
    internal class CleanFolderCommand : IRequest, ILoggedRequest
    {
        public CleanFolderCommand(string path) => Path = path;
        public string Path { get; }

        public string Trace() => $"{nameof(CleanFolderCommand)}: {Path}";
    }

    internal class CleanFolderCommandHandler : IRequestHandler<CleanFolderCommand>
    {
        private readonly IFileSystem _fileSystem;

        public CleanFolderCommandHandler(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public Task<Unit> Handle(CleanFolderCommand request, CancellationToken cancellationToken)
        {
            foreach (var file in _fileSystem.EnumerateFiles(request.Path, "*"))
                _fileSystem.FileDelete(file);
            foreach (var folder in _fileSystem.EnumerateDirectories(request.Path).Where(x => !x.EndsWith("git")))
                _fileSystem.DirectoryDelete(folder);
            return Unit.Task;
        }
    }
}