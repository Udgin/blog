using System;
using System.IO;

namespace blg.Common
{
    internal static class Utils
    {
        public static string RelativePath(string folder, string filePath)
        {
            if (filePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                throw new ArgumentException(nameof(filePath));
            }
            var pathUri = new Uri(filePath);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            var folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar)); 
        }

        public static string NormalizeString(string value) => Uri.EscapeDataString(value);
    }
}