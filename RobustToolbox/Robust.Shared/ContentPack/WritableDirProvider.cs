﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Robust.Shared.Log;
using Robust.Shared.Utility;

namespace Robust.Shared.ContentPack
{
    /// <inheritdoc />
    internal sealed class WritableDirProvider : IWritableDirProvider
    {
        /// <inheritdoc />
        public string RootDir { get; }

        /// <summary>
        /// Constructs an instance of <see cref="WritableDirProvider"/>.
        /// </summary>
        /// <param name="rootDir">Root file system directory to allow writing.</param>
        public WritableDirProvider(DirectoryInfo rootDir)
        {
            // FullName does not have a trailing separator, and we MUST have a separator.
            RootDir = rootDir.FullName + Path.DirectorySeparatorChar.ToString();
        }

        #region File Access

        /// <inheritdoc />
        public void CreateDir(ResourcePath path)
        {
            var fullPath = GetFullPath(path);
            Directory.CreateDirectory(fullPath);
        }

        /// <inheritdoc />
        public void Delete(ResourcePath path)
        {
            var fullPath = GetFullPath(path);
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
            else if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        /// <inheritdoc />
        public bool Exists(ResourcePath path)
        {
            var fullPath = GetFullPath(path);
            return Directory.Exists(fullPath) || File.Exists(fullPath);
        }

        /// <inheritdoc />
        public (IEnumerable<ResourcePath> files, IEnumerable<ResourcePath> directories) Find(string pattern, bool recursive = true)
        {
            var rootLen = RootDir.Length - 1;
            var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var files = Directory.GetFiles(RootDir, pattern, option);
            var dirs = Directory.GetDirectories(RootDir, pattern, option);

            var resFiles = new List<ResourcePath>(files.Length);
            var resDirs = new List<ResourcePath>(dirs.Length);

            foreach (var file in files)
            {
                resFiles.Add(new ResourcePath(file.Substring(rootLen), ResourcePath.SYSTEM_SEPARATOR).ChangeSeparator("/"));
            }

            foreach (var dir in dirs)
            {
                resDirs.Add(new ResourcePath(dir.Substring(rootLen), ResourcePath.SYSTEM_SEPARATOR).ChangeSeparator("/"));
            }

            return (resFiles, resDirs);
        }

        public IEnumerable<string> DirectoryEntries(ResourcePath path)
        {
            var fullPath = GetFullPath(path);

            if (!Directory.Exists(fullPath))
                yield break;

            foreach (var entry in Directory.EnumerateFileSystemEntries(fullPath))
            {
                yield return Path.GetRelativePath(fullPath, entry);
            }
        }

        /// <inheritdoc />
        public bool IsDir(ResourcePath path)
        {
            return Directory.Exists(GetFullPath(path));
        }

        /// <inheritdoc />
        public Stream Open(ResourcePath path, FileMode fileMode, FileAccess access, FileShare share)
        {
            var fullPath = GetFullPath(path);
            return File.Open(fullPath, fileMode, access, share);
        }

        public IWritableDirProvider OpenSubdirectory(ResourcePath path)
        {
            if (!IsDir(path))
                throw new FileNotFoundException();

            var dirInfo = new DirectoryInfo(GetFullPath(path));
            return new WritableDirProvider(dirInfo);
        }

        /// <inheritdoc />
        public void Rename(ResourcePath oldPath, ResourcePath newPath)
        {
            var fullOldPath = GetFullPath(oldPath);
            var fullNewPath = GetFullPath(newPath);
            File.Move(fullOldPath, fullNewPath);
        }

        #endregion

        public string GetFullPath(ResourcePath path)
        {
            if (!path.IsRooted)
            {
                throw new ArgumentException("Path must be rooted.");
            }

            return GetFullPath(RootDir, path);
        }

        private static string GetFullPath(string root, ResourcePath path)
        {
            var relPath = path.Clean().ToRelativeSystemPath();
            if (relPath.Contains("\\..") || relPath.Contains("/.."))
            {
                // Hard cap on any exploit smuggling a .. in there.
                // Since that could allow leaving sandbox.
                throw new InvalidOperationException("This branch should never be reached.");
            }

            return Path.GetFullPath(Path.Combine(root, relPath));
        }
    }
}
