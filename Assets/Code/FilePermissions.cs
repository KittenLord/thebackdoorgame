using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity.VisualScripting;

[JsonConverter(typeof(StringEnumConverter))]
public enum FilePermission
{
    Read, Write, Delete, DeleteInner, Manage, ManageInner, Inspect, Create
}

public class FilePermissions
{
    [JsonProperty("Permissions")] private Dictionary<FilePermission, List<string>> permissions = new();

    public List<string> this[FilePermission p]
    {
        get 
        { 
            if(!permissions.ContainsKey(p)) permissions[p] = new();
            return permissions[p]; 
        }
        set { permissions[p] = value; }
    }

    public FilePermissions GetOverridden(FilePermissions overrides)
    {
        var newPerms = new FilePermissions();
        foreach(var key in this.permissions.Keys)
        {
            // this is disgusting but its too late to change now
            if(key == FilePermission.Manage || key == FilePermission.Delete) continue;
            if(key == FilePermission.ManageInner)
                newPerms[FilePermission.Manage] = new(this[key]);
            else if(key == FilePermission.DeleteInner)
                newPerms[FilePermission.Delete] = new(this[key]);
            newPerms[key] = new(this[key]);
        }
        foreach(var key in overrides.permissions.Keys) if(overrides[key].Count > 0) newPerms[key] = new(overrides[key]);
        return newPerms;
    }

    private bool FitSingle(ComputerAccess access, FilePermission permission) 
    {
        var list = this[permission];
        // if(permission == FilePermission.Manage) list.AddRange(this[FilePermission.ManageInner]);
        // if(permission == FilePermission.DeleteInner) list.AddRange(this[FilePermission.DeleteInner]);
        if(list.Count <= 0) return true;
        if(list.Any(p => p == access.Username || (int.TryParse(p, out var i) && i <= access.AccessLevel))) return true;
        list.RemoveAll(l => !l.Contains(" "));
        return list.Select(l => l.Split(" ")).Any(l => int.Parse(l[0]) <= access.AccessLevel && l[1] == access.Username);
    }
    public bool Fit(ComputerAccess access, params FilePermission[] permissions) => (access.Username == "root") || permissions.All(p => FitSingle(access, p));

    public FilePermissions() {}
    public FilePermissions(params (FilePermission p, List<string> a)[] permissions) { foreach(var p in permissions) this[p.p] = p.a; }
}