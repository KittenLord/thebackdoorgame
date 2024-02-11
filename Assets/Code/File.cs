using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class File
{
    public string FilePath { get; private set; }    
    public bool IsDirectory { get; private set; }
    public FilePermissions Permissions { get; private set; }

    public string FileContents { get; private set; }
}