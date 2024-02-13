using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

public enum FileSystemError
{
    DirectoryDoesntExist,
    Forbidden,
    InvalidPath,
    FatalError,
    BadFileName,
    FileIsDirectory
}

public partial class Computer
{
    public class FileNavigator
    {
        private Computer computer;
        private ComputerAccess access;
        public FileNavigator(Computer computer, ComputerAccess access) { this.computer = computer; this.access = access; }


        public bool IsValidFileName(string name)
        {
            if(name == "" || name == null) return false;
            if(name == "~") return true;
            return name.All(c => char.IsLetterOrDigit(c) || "._-()".Contains(c));
        }

        public bool IsValidPath(string path)
        {
            if(path == "" || path == null) return false;
            var split = path.Split("/").ToList();
            if(split.First() == "") split.RemoveAt(0);
            if(split.Last() == "") split.RemoveAt(split.Count - 1);
            if(split.Count <= 0) return false;
            if(split.Count(f => f == "~") > 1) return false;
            if(split.Count(f => f == "~") == 1 && split.First() != "~") return false;
            return split.All(s => IsValidFileName(s)); 
        }
        public bool IsValidAbsolutePath(string path)
        {
            return path.StartsWith("~") && IsValidPath(path);
        }

        public string SanitizePath(string path)
        {
            if(!IsValidPath(path)) return null;
            var buffer = path.ToCharArray().ToList();
            if(buffer.First() == '/') buffer.RemoveAt(0);
            if(buffer.Last() == '/') buffer.RemoveAt(buffer.Count - 1);
            return new string(buffer.ToArray());
        }

        public List<string> GetPathWalkthough(string path)
        {
            var split = path.Split("/");
            var result = split.Select(s => "").ToList();

            for(int i = 0; i < split.Length; i++)
            {
                for(int j = 0; j <= i; j++)
                    result[i] += split[j] + (j != i ? "/" : "");
            }

            return result;
        }

        public FileSystemError? GetFilePermissions(string path, out FilePermissions permissions)
        {
            permissions = null;
            var walkthrough = GetPathWalkthough(path);
            foreach(var w in walkthrough) Debug.Log(w);
            if(walkthrough.Any(w => !computer.FileSystem.Any(file => file.Path == w))) return FileSystemError.FatalError;
            var walkthroughFiles = walkthrough.Select(w => computer.FileSystem.Find(f => f.Path == w));

            var perms = walkthroughFiles.First().Permissions;
            foreach(var wf in walkthroughFiles) perms = perms.GetOverridden(wf.Permissions);
            permissions = perms;
            return null;
        }

        public string Path { get; private set; }
        public FileSystemError? Goto(string path)
        {
            if(!IsValidAbsolutePath(path)) return FileSystemError.InvalidPath;
            path = SanitizePath(path);

            if(!computer.FileSystem.Any(file => file.Path == path && file.IsDirectory)) return FileSystemError.DirectoryDoesntExist;

            var result = GetFilePermissions(path, out var permissions);
            if(result is not null) return result;
            if(!permissions.Fit(this.access, FilePermission.Inspect)) return FileSystemError.Forbidden;

            this.Path = path;
            return null;
        }

        public string? GetModifiedPath(string oldPath, string modifierPath)
        {
            if(modifierPath == "" || modifierPath == ".") return oldPath;
            if(IsValidAbsolutePath(modifierPath)) return modifierPath;
            if(!IsValidPath(modifierPath)) return null;

            List<string> oldPathSplit = oldPath.Split("/").ToList();
            List<string> modPathSplit = modifierPath.Split("/").ToList();

            foreach(var s in modPathSplit)
            {
                if(s == ".." && oldPathSplit.Count <= 0) return null;
                else if(s == "..") oldPathSplit.RemoveAt(oldPathSplit.Count - 1);
                else if(s == ".") continue;
                else oldPathSplit.Add(s);
            }

            return string.Join("/", oldPathSplit);
        }

        public FileSystemError? ReadFile(string path, out string contents)
        {
            contents = "";
            path = GetModifiedPath(this.Path, path);
            Debug.Log(path);
            if(path is null) return FileSystemError.InvalidPath;
            var result = GetFilePermissions(path, out var permissions);
            if(result is not null) return result;
            if(!permissions.Fit(this.access, FilePermission.Read)) return FileSystemError.Forbidden;
            var file = GetFile(path);
            if(file.IsDirectory) return FileSystemError.FileIsDirectory;
            contents = file.Contents;
            return null;
        }

        public bool CanWriteToFile(string path)
        {
            var result = GetFilePermissions(path, out var permissions);
            Debug.Log(result.ToString() ?? "daiwudbauidbaw");
            if(result is not null) return false;
            return permissions.Fit(this.access, FilePermission.Write);
        }

        public FileSystemError? WriteFile(string path, string contents)
        {
            path = GetModifiedPath(this.Path, path);
            if(path is null) return FileSystemError.InvalidPath;
            var result = GetFilePermissions(path, out var permissions);
            if(result is not null) return result;
            if(!permissions.Fit(this.access, FilePermission.Write)) return FileSystemError.Forbidden;
            var file = GetFile(path);
            if(file.IsDirectory) return FileSystemError.FileIsDirectory;
            file.Contents = contents;
            return null;
        }

        public FileSystemError? Navigate(string path)
        {
            var newPath = GetModifiedPath(this.Path, path);

            Debug.Log(path);
            if(!computer.FileSystem.Any(file => file.Path == newPath && file.IsDirectory)) return FileSystemError.DirectoryDoesntExist;

            var result = GetFilePermissions(newPath, out var permissions);
            if(result is not null) return result;
            if(!permissions.Fit(this.access, FilePermission.Inspect)) return FileSystemError.Forbidden;

            this.Path = newPath;
            return null;
        }

        public File? GetFile(string path)
        {
            return computer.FileSystem.Find(f => f.Path == path);
        }

        public bool FileExists(string path) => GetFile(path) is not null;

        public FileSystemError? CreateFile(string path, bool isDirectory, out string filePath)
        {
            filePath = "";
            var oldPath = this.Path;
            if(!IsValidPath(path)) return FileSystemError.InvalidPath;
            
            var split = path.Split("/").ToList();
            var fileName = split.Last();
            if(!IsValidFileName(fileName) || fileName == "~" || fileName.StartsWith("..")) return FileSystemError.BadFileName;
            split.RemoveAt(split.Count - 1);
            path = string.Join("/", split);

            var result = this.Navigate(path);
            if(result is not null) return result;

            var result2 = GetFilePermissions(this.Path, out var permissions);
            if(result2 is not null) return result2;

            if(!permissions.Fit(this.access, FilePermission.Create)) return FileSystemError.Forbidden;

            filePath = this.Path + "/" + fileName;
            computer.FileSystem.Add(new File(filePath, new(), isDirectory));
            this.Navigate(oldPath);
            return null;
        }

        public FileSystemError? Back()
        {
            if(this.Path == "~") return FileSystemError.InvalidPath;
            var split = this.Path.Split("/").ToList();
            split.RemoveAt(split.Count - 1);
            var path = string.Join("/", split);

            var result = GetFilePermissions(path, out var permissions);
            if(result is not null) return result;
            if(!permissions.Fit(this.access, FilePermission.Inspect)) return FileSystemError.Forbidden;

            this.Path = path;
            return null;
        }

        public List<string> List(string prefix)
        {
            prefix += "/";
            var files = computer.FileSystem
                .Where(f => f.Path.StartsWith(prefix))
                .Where(f => {
                    var result = this.GetFilePermissions(f.Path, out var p);
                    return result is null && p.Fit(this.access, FilePermission.Inspect);
                })
                .Where(f => IsValidFileName(f.Path.Replace(prefix, "")));
            return files.Select(f => f.Path).ToList();
        }



        public class TreeNode<T>
        {
            public T Value { get; private set; }
            public List<TreeNode<T>> Branches { get; private set; } = new();

            public TreeNode(T value)
            {
                Value = value;
                Branches = new();
            }
        }

        public FileSystemError? GetTree(string path, out TreeNode<string> tree)
        {
            tree = new(null);
            path = GetModifiedPath(this.Path, path);
            if(path is null) return FileSystemError.InvalidPath;

            var result = this.GetFilePermissions(path, out var p);
            if(!p.Fit(this.access, FilePermission.Inspect)) return FileSystemError.Forbidden;

            var file = this.GetFile(path);
            if(file is null) return FileSystemError.DirectoryDoesntExist;

            tree = new(file.Path);

            void RecursiveAdd(TreeNode<string> focus)
            {
                var slashes = focus.Value.Count(c => c == '/');
                var branches = computer.FileSystem
                    .Where(f => f.Path.StartsWith(focus.Value))
                    .Where(f => f.Path.Count(c => c == '/') - slashes == 1)
                    .Where(f => {
                        var result = this.GetFilePermissions(f.Path, out var p);
                        return result is null && p.Fit(this.access, FilePermission.Inspect);
                    });

                foreach(var branch in branches) 
                {
                    var b = new TreeNode<string>(branch.Path);
                    focus.Branches.Add(b);
                    RecursiveAdd(b);
                }
            }

            RecursiveAdd(tree);
            return null;
        }
    }
}