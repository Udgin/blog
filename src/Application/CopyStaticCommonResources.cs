using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace blg.Application
{
    internal class CopyStaticCommonResourcesCommand : IRequest
    {
        public CopyStaticCommonResourcesCommand(string path, string targetFolder)
        {
            Path = path;
            TargetFolder = targetFolder;
        }
        public string Path { get; }
        public string TargetFolder { get; }
    }

    internal class CopyStaticCommonResourcesCommandHandler : IRequestHandler<CopyStaticCommonResourcesCommand>
    {
        private readonly IFileSystem _fileSystem;

        public CopyStaticCommonResourcesCommandHandler(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        public Task<Unit> Handle(CopyStaticCommonResourcesCommand request, CancellationToken cancellationToken)
        {
            _fileSystem.CopyFile(
                request.Path,
                PathToStaticResource(request));

            return Unit.Task;
        }

        private string PathToStaticResource(CopyStaticCommonResourcesCommand request) =>
            Path.Combine(request.TargetFolder, Path.GetFileName(request.Path));
    }
}