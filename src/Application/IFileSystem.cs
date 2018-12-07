using System.Collections.Generic;
using System.Threading.Tasks;

namespace blg.Application
{
    interface IFileSystem
    {
        Task<string[]> ReadAllLinesAsync(string path);
        Task<string> ReadAllTextAsync(string path);
        string ReadAllText(string path);
        bool DirectoryExists(string path);
        void DirectoryDelete(string path);
        void DirectoryCreate(string path);
        IEnumerable<string> EnumerateFiles(string path, string pattern = "*.md");
        Task WriteAllTextAsync(string path, string text);
        IEnumerable<string> EnumerateDirectories(string path);
        void CopyFile(string source, string target);
        bool FileExists(string path);
        void FileDelete(string path);
    }
}