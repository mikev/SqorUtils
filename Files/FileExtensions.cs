using System.IO;

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
    }
}
