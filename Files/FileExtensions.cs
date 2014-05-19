using System.IO;
using System;
using System.Linq;

namespace Sqor.Utils.Files
{
    public static class FileExtensions
    {
        /// <summary>
        /// Deletes all files and subdirectories from this directory, leaving the directory
        /// in tact.
        /// </summary>
        /// <param name="directory">Directory.</param>
        public static void Empty(this DirectoryInfo directory)
        {
            foreach (var subDirectory in directory.GetDirectories())
            {
                subDirectory.DeleteAll();
            }
            foreach (var file in directory.GetFiles())
            {
                file.Delete();
            }            
        }
        
        /// <summary>
        /// Deletes this directory and all its files and sub-directories.
        /// </summary>
        public static void DeleteAll(this DirectoryInfo directory)
        {
            foreach (var subDirectory in directory.GetDirectories())
            {
                subDirectory.DeleteAll();
            }
            foreach (var file in directory.GetFiles())
            {
                file.Delete();
            }
            directory.Delete();
        }
        
        public static void CopyTo(this DirectoryInfo directory, string destination)
        {
            var copy = destination + Path.DirectorySeparatorChar + directory.Name;
            directory.CopyContentsTo(copy);
        }
        
        public static void CopyContentsTo(this DirectoryInfo directory, string destination)
        {
            Directory.CreateDirectory(destination);

            foreach (var subDirectory in directory.GetDirectories())
            {
                subDirectory.CopyTo(destination);
            }        
            foreach (var file in directory.GetFiles())
            {
                file.CopyTo(destination + Path.DirectorySeparatorChar + file.Name);
            }
        }

        /// ---------------------------------------------------------------
        /// <summary> Convert from a <i>URL</i> to a <see cref="FileInfo"/>.</summary>
        /// <param name="url">File URL.
        /// </param>
        /// <returns> The equivalent <see cref="FileInfo"/> object, or <i>null</i> if the URL's protocol
        /// is not <i>file</i>
        /// </returns>
        /// ---------------------------------------------------------------
        public static FileInfo ToFile(this System.Uri url)
        {
            if (url.Scheme.Equals("file") == false)
            {
                return null;
            }
            else
            {
                String filename = url.PathAndQuery.Replace('/', Path.DirectorySeparatorChar);
                return new FileInfo(filename);
            }
        }

        public static string GetExtension(this string fileName)
        {
            var parts = fileName.Split('.');
            return parts.Last();
        }
    }
}
