using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using blg.Application;

namespace blg.Infrastructure
{
    internal class FileSystem : IFileSystem
    {
        public async Task<string[]> ReadAllLinesAsync(string path) => await File.ReadAllLinesAsync(path);
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public void DirectoryDelete(string path) => Directory.Delete(path, true);
        public void DirectoryCreate(string path) => Directory.CreateDirectory(path);
        public IEnumerable<string> EnumerateFiles(string path, string pattern = "*.md") => Directory.EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly);
        public async Task WriteAllTextAsync(string path, string text) => await File.WriteAllTextAsync(path, text);
        public IEnumerable<string> EnumerateDirectories(string path) => Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly);
        public string ReadAllText(string path) => File.ReadAllText(path);
        public async Task<string> ReadAllTextAsync(string path) => await File.ReadAllTextAsync(path);
        public void CopyFile(string source, string target) => File.Copy(source, target);
        public bool FileExists(string path) => File.Exists(path);
        public void FileDelete(string path) => File.Delete(path);
    }
}