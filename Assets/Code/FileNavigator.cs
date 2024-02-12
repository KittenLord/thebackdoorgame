using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.VisualScripting;

public enum FileSystemError
{
    DirectoryDoesntExist,
    Forbidden,
    InvalidPath,
    FatalError
}

public partial class Computer
{
    public class FileNavigator
    {
        private Computer computer;
        private ComputerAccess access;
        public FileNavigator(Computer computer, ComputerAccess access) { this.computer = computer; this.access = access; }


        public bool ValidateNodeName(string name)
        {
            return name.All(c => char.IsLetterOrDigit(c) || "_.".Contains(c)) && name.Length > 0;
        }

        public string CreatePath(params string[] args) 
        {
            if(args.Any(arg => !ValidateNodeName(arg))) return "";
            return string.Join("/", args);
        }

        public string CreateAbsolutePath(params string[] args)
        {
            return "~/" + CreatePath(args.ToArray());
        }

        public string? ValidateAbsolutePath(string path)
        {
            if(path == "~" || path == "~/") return "~";
            var split = path.Split("/").ToList();
            if(split.First() == "~") split.Remove("~"); else return null;
            var merged = CreateAbsolutePath(split.ToArray());
            return merged;
        }

        public List<string> GetPathWalkthough(string path)
        {
            var split = path.Split("/");
            var result = split.Select(s => "").ToList();

            for(int i = 0; i < split.Length; i++)
                for(int j = 0; j <= i; j++)
                    result[i] += split[j] + (j != i ? "/" : "");
            
            return result;
        }

        public string Path { get; private set; }
        public FileSystemError? Goto(string path)
        {
            path = ValidateAbsolutePath(path);
            if(path is null) return FileSystemError.InvalidPath;
            UnityEngine.Debug.Log(path);

            if(!computer.FileSystem.Any(file => file.Path == path && file.IsDirectory)) return FileSystemError.DirectoryDoesntExist;
            var walkthrough = GetPathWalkthough(path);
            UnityEngine.Debug.Log(string.Join(" ", walkthrough));
            if(walkthrough.Any(w => !computer.FileSystem.Any(file => file.Path == w && file.IsDirectory))) return FileSystemError.FatalError;
            var walkthroughFiles = walkthrough.Select(w => computer.FileSystem.Find(f => f.Path == w));
            var perms = walkthroughFiles.First().Permissions;
            UnityEngine.Debug.Log(JsonConvert.SerializeObject(perms));
            foreach(var wf in walkthroughFiles) perms = perms.GetOverridden(wf.Permissions);
            UnityEngine.Debug.Log(JsonConvert.SerializeObject(perms));
            if(!perms.Fit(this.access, FilePermission.Inspect)) return FileSystemError.Forbidden;
            this.Path = path;
            return null;
        }
    }
}