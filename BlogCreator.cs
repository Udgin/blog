using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Markdig;

namespace blg
{
    internal interface IBlogCreator
    {
        Task Generate();
    }

    internal class BlogCreator : IBlogCreator
    {
        private readonly IArticlePageCreator _articlePageCreator;
        private readonly IFileSystem _fileSystem;
        private readonly IBlogConfiguration _configuration;
        private readonly IIndexPageCreator _indexPageCreator;

        private string _cardTemplate { get; set; }

        public BlogCreator(
            IFileSystem fileSystem,
            IBlogConfiguration configuration,
            IIndexPageCreator indexPageCreator,
            IArticlePageCreator articlePageCreator
            )
        {
            _articlePageCreator = articlePageCreator;
            _fileSystem = fileSystem;
            _configuration = configuration;
            _indexPageCreator = indexPageCreator;
        }
        private string PathToFolderImage =>
            Path.Combine(_configuration.TargetFolder, Path.GetFileName(_configuration.FolderImagePath));
        private string PathToPrismCSS =>
            Path.Combine(_configuration.TargetFolder, Path.GetFileName(_configuration.PrismCSS));
        private string PathToPrismJS =>
            Path.Combine(_configuration.TargetFolder, Path.GetFileName(_configuration.PrismJS));
        public async Task Generate()
        {
            _cardTemplate = await _fileSystem.ReadAllTextAsync(_configuration.CardTemplatePath);

            if (_fileSystem.DirectoryExists(_configuration.TargetFolder))
                _fileSystem.DirectoryDelete(_configuration.TargetFolder);
            _fileSystem.DirectoryCreate(_configuration.TargetFolder);

            _fileSystem.CopyFile(
                _configuration.FolderImagePath,
                PathToFolderImage);

            _fileSystem.CopyFile(
                _configuration.PrismCSS,
                PathToPrismCSS);

            _fileSystem.CopyFile(
                _configuration.PrismJS,
                PathToPrismJS);

            await CreateFolder(string.Empty);
        }

        private string GetTargetFolderPath(string folderPath)
        {
            var folderTail = folderPath.Substring(_configuration.SourceFolder.Length);
            return Path.Combine(_configuration.TargetFolder, folderTail.TrimStart('\\').TrimStart('/'));
        }

        private async Task<CardEntity> CreateFolder(string relativeFolderPath)
        {
            var fullPathToFolder = Path.Combine(_configuration.ArticlesFolder, relativeFolderPath);
            var fullPathToTargetFolder = Path.Combine(_configuration.TargetFolder, relativeFolderPath);

            if (!_fileSystem.DirectoryExists(fullPathToTargetFolder))
            {
                _fileSystem.DirectoryCreate(fullPathToTargetFolder);
            }

            var filePathes = _fileSystem.EnumerateFiles(fullPathToFolder);
            var cards = new List<CardEntity>();

            foreach (var directory in _fileSystem.EnumerateDirectories(fullPathToFolder))
            {
                var folderCard = await CreateFolder(Utils.RelativePath(_configuration.ArticlesFolder, directory));
                cards.Add(folderCard);
            }

            foreach (var path in filePathes)
            {
                var fileCard = await _articlePageCreator.Create(Utils.RelativePath(_configuration.ArticlesFolder, path));
                cards.Add(fileCard);
            }

            await _indexPageCreator.Create(relativeFolderPath, cards);

            return new CardEntity {
                Tags = new Dictionary<string, string> {
                    ["Title"] = Path.GetFileNameWithoutExtension(relativeFolderPath)
                },
                RelativePath = Path.GetFileNameWithoutExtension(relativeFolderPath) + "/Index.html",
                ImageRelativePath = Utils.RelativePath (
                    Path.GetDirectoryName(relativeFolderPath) == null ?
                        _configuration.TargetFolder :
                        Path.Combine(_configuration.TargetFolder, Path.GetDirectoryName(relativeFolderPath)),
                    PathToFolderImage
                )
            };
        }
    }
}