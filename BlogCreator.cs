using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private string PathToStaticResource(string path) =>
            Path.Combine(_configuration.TargetFolder, Path.GetFileName(path));
        public async Task Generate()
        {
            CleanUpFolder(_configuration.TargetFolder);

            CopyStaticCommonResources();

            await CreateFolder(string.Empty);
        }
        private void CopyStaticCommonResources()
        {
            _fileSystem.CopyFile(
                _configuration.FolderImagePath,
                PathToStaticResource(_configuration.FolderImagePath));

            _fileSystem.CopyFile(
                _configuration.PrismCSS,
                PathToStaticResource(_configuration.PrismCSS));

            _fileSystem.CopyFile(
                _configuration.PrismJS,
                PathToStaticResource(_configuration.PrismJS));

            _fileSystem.CopyFile(
                _configuration.Favicon,
                PathToStaticResource(_configuration.Favicon));
        }

        private void CleanUpFolder(string path)
        {
            foreach (var file in _fileSystem.EnumerateFiles(path, "*"))
                _fileSystem.FileDelete(file);
            foreach (var folder in _fileSystem.EnumerateDirectories(path).Where(x => !x.EndsWith("git")))
                _fileSystem.DirectoryDelete(folder);
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
                RelativePath = Path.GetFileNameWithoutExtension(relativeFolderPath) + "/index.html",
                ImageRelativePath = Utils.RelativePath (
                    Path.GetDirectoryName(relativeFolderPath) == null ?
                        _configuration.TargetFolder :
                        Path.Combine(_configuration.TargetFolder, Path.GetDirectoryName(relativeFolderPath)),
                    PathToStaticResource(_configuration.FolderImagePath)
                )
            };
        }
    }
}