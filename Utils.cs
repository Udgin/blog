using System;
using System.IO;

namespace blg
{
    internal static class Utils
    {
        public static string RelativePath(string folder, string filePath)
        {
            Uri pathUri = new Uri(filePath);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar)); 
        }

        public static string NormalizeString(string value) => Uri.EscapeDataString(value);
    }
}