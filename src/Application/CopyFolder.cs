using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using blg.Common;
using blg.Domain;
using MediatR;

namespace blg.Application
{
    internal class CopyFolderCommand : IRequest<CardEntity>, ILoggedRequest
    {
        public CopyFolderCommand(string folderPath, string sourceFolder)
        {
            SourceFolder = sourceFolder;
            FolderPath = folderPath;
        }
        public string SourceFolder { get; }
        public string FolderPath { get; }

        public string Trace() => $"{nameof(CopyFolderCommand)}: {SourceFolder}, {FolderPath}";
    }
    internal class CopyFolderCommandHandler : IRequestHandler<CopyFolderCommand, CardEntity>
    {
        public IMediator _mediator { get; }

        private readonly IFileSystem _fileSystem;

        public CopyFolderCommandHandler(IMediator mediator, IFileSystem fileSystem)
        {
            _mediator = mediator;
            _fileSystem = fileSystem;
        }
        public async Task<CardEntity> Handle(CopyFolderCommand request, CancellationToken cancellationToken)
        {
            var configuration = await _mediator.Send(new GetConfigurationCommand(request.SourceFolder));

            var fullPathToFolder = Path.Combine(configuration.ArticlesFolder, request.FolderPath);
            var fullPathToTargetFolder = Path.Combine(configuration.TargetFolder, request.FolderPath);

            if (!_fileSystem.DirectoryExists(fullPathToTargetFolder))
            {
                _fileSystem.DirectoryCreate(fullPathToTargetFolder);
            }

            var filePathes = _fileSystem.EnumerateFiles(fullPathToFolder);
            var cards = new List<CardEntity>();

            foreach (var directory in _fileSystem.EnumerateDirectories(fullPathToFolder))
            {
                cards.Add(await _mediator.Send(
                    new CopyFolderCommand(
                        Utils.RelativePath(configuration.ArticlesFolder, directory),
                        request.SourceFolder)));
            }

            foreach (var path in filePathes)
            {
                cards.Add(await _mediator.Send(
                    new CreateArticlePageCommand(Utils.RelativePath(configuration.ArticlesFolder, path), request.SourceFolder)
                ));
            }

            var pathToFolderWithIndexFile = Path.Combine(configuration.TargetFolder, request.FolderPath);

            return await _mediator.Send(
                new CreateIndexPageCommand(
                    request.SourceFolder,
                    cards,
                    pathToFolderWithIndexFile,
                    string.IsNullOrEmpty(request.FolderPath) ?
                        configuration.BlogTitle :
                        Path.GetFileNameWithoutExtension(pathToFolderWithIndexFile)
                    )
            );
        }
    }
}