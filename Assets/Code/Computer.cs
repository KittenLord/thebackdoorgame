using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

public class Process
{
    public int Id { get; private set; }
    public Application Application { get; private set; }
    public ComputerAccess Access { get; private set; }

    public Process(int id, Application application, ComputerAccess access)
    {
        Id = id;
        Application = application;
        Access = access;
    }
}

public class ComputerAccess
{

    public ComputerAccess(string username, int accessLevel)
    {
        this.Username = username;
        this.AccessLevel = accessLevel;
    }

    public string Username { get; private set; }
    public int AccessLevel { get; private set; }

    public override string ToString()
    {
        return AccessLevel + " " + Username;
    }

    public ComputerAccess Copy() => new ComputerAccess(Username, AccessLevel);
    public ComputerAccess Modify(int accessLevel) => new ComputerAccess(Username, accessLevel);
}

public class ComputerUser
{
    public string Username { get; private set; }
    public string Password { get; private set; }

    public ComputerUser(string username, string password)
    {
        Username = username;
        Password = password;
    }
}

public partial class Computer
{
    [JsonProperty] public string Ip { get; private set; }
    [JsonProperty] private Dictionary<string, ComputerUser> Users { get; set; } = new();
    [JsonProperty] private List<File> FileSystem { get; set; } = new();

    private Dictionary<int, Process> Processes { get; set; } = new();

    private bool AddProcess(string username, string password, int accessLevel, Application application)
    {
        if(!Users.ContainsKey(username)) return false;
        var user = Users[username];
        if(user.Password != password) return false;
        return AddProcess(new ComputerAccess(username, accessLevel), application); 
    }

    private bool AddProcess(int pid, int accessLevel, Application application)
    {
        if(!Processes.ContainsKey(pid)) return false;
        var p = Processes[pid];
        return this.AddProcess(p.Access.Modify(accessLevel), application);
    }

    private bool AddProcess(ComputerAccess access, Application application)
    {
        if(Processes.Values.Any(v => v.Application == application)) return false;
        int pid = Random.Range(0, int.MaxValue);
        while(Processes.ContainsKey(pid)) pid = Random.Range(0, int.MaxValue);

        Processes.Add(pid, new Process(pid, application, access));
        application.ProcessId = pid;
        return true;
    }

    private void KillProcess(int pid)
    {
        if(!Processes.ContainsKey(pid)) return;
        var p = Processes[pid];
        Processes.Remove(pid);
        p.Application.OnKilled();
    }
}
