using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.VisualScripting;

public enum FilePermission
{
    Read, Write, Delete, Manage
}

public class FilePermissions
{
    [JsonProperty("Permissions")] private Dictionary<FilePermission, List<string>> permissions = new();

    public List<string> this[FilePermission p]
    {
        get { if(!permissions.ContainsKey(p)) permissions[p] = new(); return permissions[p]; }
        set { permissions[p] = value; }
    }

    public FilePermissions GetOverridden(FilePermissions overrides)
    {
        var newPerms = new FilePermissions();
        foreach(var key in this.permissions.Keys) newPerms[key] = new(this[key]);
        foreach(var key in overrides.permissions.Keys) newPerms[key] = new(overrides[key]);
        return newPerms;
    }

    public FilePermissions() {}
    public FilePermissions(params (FilePermission p, List<string> a)[] permissions) { foreach(var p in permissions) this[p.p] = p.a; }
}