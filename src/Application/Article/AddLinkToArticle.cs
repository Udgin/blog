using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace blg.Application
{
    internal class AddLinkToArticleCommand : IRequest<string>
    {
        public AddLinkToArticleCommand(
            string content,
            string fullPathToArticle,
            string link,
            string fullPathToHtmlArticle
        )
        {
            Content = content;
            FullPathToArticle = fullPathToArticle;
            Link = link;
            FullPathToHtmlArticle = fullPathToHtmlArticle;
        }

        public string Content { get; }
        public string FullPathToArticle { get; }
        public string Link { get; }
        public string FullPathToHtmlArticle { get; }
    }

    internal class AddLinkToArticleHandler : IRequestHandler<AddLinkToArticleCommand, string>
    {
        private readonly IFileSystem _fileSystem;

        public AddLinkToArticleHandler(
            IFileSystem fileSystem
        )
        {
            _fileSystem = fileSystem;
        }
        public Task<string> Handle(AddLinkToArticleCommand request, CancellationToken cancellationToken)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(request.FullPathToHtmlArticle);

            var folderForAssets = Path.GetDirectoryName(request.FullPathToHtmlArticle);

            var fullPathToAsset = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(request.FullPathToArticle), request.Link));

            if (!_fileSystem.FileExists(fullPathToAsset)) return Task.FromResult(request.Content);

            var targetPathForAsset = Path.Combine(folderForAssets, fileNameWithoutExtension, Path.GetFileName(fullPathToAsset));

            if (!_fileSystem.DirectoryExists(Path.GetDirectoryName(targetPathForAsset)))
            {
                _fileSystem.DirectoryCreate(Path.GetDirectoryName(targetPathForAsset));
            }

            _fileSystem.CopyFile(fullPathToAsset, targetPathForAsset);

            return Task.FromResult(request.Content.Replace(
                request.Link,
                Path.Combine(fileNameWithoutExtension, Path.GetFileName(fullPathToAsset))
            ));
        }
    }
}