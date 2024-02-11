using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class File
{
    public string Path { get; private set; }    
    public FilePermissions Permissions { get; private set; }
    public bool IsDirectory { get; private set; }

    public string Contents { get; private set; }

    public File(string path, FilePermissions permissions, bool isDirectory = false, string contents = "")
    {
        Path = path;
        IsDirectory = isDirectory;
        Permissions = permissions;
        Contents = contents;
    }
}