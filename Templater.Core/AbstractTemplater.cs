using System.IO.Compression;

namespace Templater.Core
{
    public abstract class AbstractTemplater
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractTemplater"/> class.
        /// </summary>
        /// <param name="longName">The long name.</param>
        /// <param name="shortName">The short name.</param>
        public AbstractTemplater(string longName, string shortName)
        {
            LongName = longName;
            ShortName = shortName;
        }

        /// <summary>
        /// Gets the long name of the templater.
        /// </summary>
        /// <value>
        /// The long name.
        /// </value>
        public string LongName { get; private set; }

        /// <summary>
        /// Gets the short name of the templater.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public string ShortName { get; private set; }

        /// <summary>
        /// Checks whether the directory is valid for this templater.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns>True if valid, False otherwise.</returns>
        public abstract bool DirectoryValidForTemplater(string directory);

        /// <summary>
        /// Prepares the template.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The preperation result.</returns>
        public abstract string Prepare(PrepareOptions options);

        /// <summary>
        /// Generates a solution from a template.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The generation result.</returns>
        public abstract string Generate(GenerateOptions options);

        /// <summary>
        /// Copies a directory.
        /// </summary>
        /// <param name="oldPath">The old path.</param>
        /// <param name="newPath">The new path.</param>
        /// <param name="excludedDirectories">The excluded directories.</param>
        protected void CopyDirectory(string oldPath, string newPath, List<string> excludedDirectories)
        {
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            // Copy over items
            var files = Directory.GetFiles(oldPath);
            for (var i = 0; i < files.Length; i++)
            {
                var path = Path.Combine(newPath, Path.GetFileName(files[i]));
                File.Copy(files[i], path, true);
            }

            // Copy over sub-directories
            var directories = Directory.GetDirectories(oldPath);
            for (var i = 0; i < directories.Length; i++)
            {
                var dirName = Path.GetFileName(directories[i]);
                if (!excludedDirectories.Contains(dirName.ToLowerInvariant()))
                {
                    var path = Path.Combine(newPath, dirName);
                    CopyDirectory(directories[i], path, excludedDirectories);
                }
            }
        }

        /// <summary>
        /// Archives a directory.
        /// </summary>
        /// <param name="directoryToArchive">The directory to archive.</param>
        /// <param name="archivePath">The archive path.</param>
        /// <param name="skipCleaning">if set to <c>true</c> [skip cleaning].</param>
        protected void ArchiveDirectory(string directoryToArchive, string archivePath, bool skipCleaning)
        {
            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
            }

            ZipFile.CreateFromDirectory(directoryToArchive, archivePath);
            if (!skipCleaning)
            {
                for (var i = 0; i < 10; i++)
                {
                    try
                    {
                        Directory.Delete(directoryToArchive, true);
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(500);
                    }
                }
            }
        }
    }
}