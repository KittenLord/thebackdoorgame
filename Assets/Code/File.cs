using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class File
{
    public string Path { get; set; }    
    public FilePermissions Permissions { get; set; }
    public bool IsDirectory { get; set; }
    public bool IsObfuscated { get; set; }

    public string Contents { get; set; }

    public File(string path, FilePermissions permissions, bool isDirectory = false, bool isObfuscated = false, string contents = "")
    {
        Path = path;
        IsDirectory = isDirectory;
        IsObfuscated = isObfuscated;
        Permissions = permissions;
        Contents = contents;
    }
}