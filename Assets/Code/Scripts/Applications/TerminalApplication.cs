using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using Unity.VisualScripting;
using TreeEditor;

public class TerminalApplication : WindowApplication
{
    [SerializeField] private TMP_Text ScrollText;
    [SerializeField] private Scrollbar Scrollbar;
    [SerializeField] private TMP_InputField Input;

    private Computer.FileNavigator navigator;

    void Start()
    {
        Window.SetTitle($"Terminal @ {Handle.GetIp()}");
        Input.onSubmit.AddListener(ExecuteCommand);
        navigator = Handle.GetNavigator(ProcessId);
        if(navigator.Goto($"~/users/{Handle.GetProcess(ProcessId).Access.Username}") is not null) navigator.Goto("~");
    }

    private void ExecuteCommand(string line)
    {
        string command = Input.text;
        var prefix = Handle.GetProcess(this.ProcessId).Access.AccessLevel + "$" + Handle.GetProcess(this.ProcessId).Access.Username;
        Log($"<color=#AAAAAA>{prefix} {navigator.Path} > </color>" + Input.text);
        Input.text = "";

        var args = CommandArgumentsParser.Parse(command);
        if(args.Count <= 0) return;
        var first = args.First();
        args.RemoveAt(0);

        if(first == "cd") CdCommand(args);
        if(first == "ls") LsCommand(args);
        if(first == "file") FileCommand(args);
        if(first == "dir") DirectoryCommand(args);
        if(first == "txt") TextCommand(args);
        if(first == "perms") PermsCommand(args);
        if(first == "tree") TreeCommand(args);
    }

    private async void LogRaw(string message)
    {
        ScrollText.text += message;        

        // Refocus on the input
        EventSystem.current.SetSelectedGameObject(Input.gameObject, null);
        Input.OnPointerClick(new PointerEventData(EventSystem.current));

        // Possibly add a coroutine
        await System.Threading.Tasks.Task.Delay(50);
        Scrollbar.value = 0;
    }

    private void LogRaw(string message, string color) { LogRaw($"<color=#{color}>{message}</color>"); }
    private void Log(string message) { LogRaw((ScrollText.text == "" ? "" : "\n") + message); }
    private void Log(string message, string color) { Log($"<color=#{color}>{message}</color>"); } 
    private void LogError(string message, string color = "FF0000") => Log(message, color);
    private void LogDefaultError(string error, string color = "FF0000") => LogError($"An error occurred: {error}", color);

    private bool CheckArgumentCount(int actual, params int[] expected) 
    { 
        if(!expected.Contains(actual)) 
        {
            LogError($"Expected {string.Join(", ", expected)} argument{(expected.Last() == 1 ? "" : "s")}. Found {actual} argument{(actual == 1 ? "" : "s")}."); 
            return false;
        }
        return true;
    }

    private void CdCommand(List<string> args)
    {
        if(!CheckArgumentCount(args.Count, 1)) return;
        var result = navigator.Navigate(args[0]);
        if(result is null) Log($"Changed directory to {navigator.Path}");
        else LogDefaultError(result.ToString());
    }

    private void LsCommand(List<string> args)
    {
        if(!CheckArgumentCount(args.Count, 0, 1)) return;
        var result = navigator.List();
        if(result.Count <= 0) Log("This directory is empty."); else Log("");
        string LabelColor(File file)
        {
            if(file.IsDirectory) return "5599FF";
            if(file.Path.EndsWith(".exe")) return "11FF11";
            return "CCCCCC";
        }
        foreach(var path in result) { var file = navigator.GetFile(path); LogRaw(file.Path.Split("/").Last() + "    ", LabelColor(file)); }
    }

    private void FileCommand(List<string> args)
    {
        if(!CheckArgumentCount(args.Count, 1)) return;
        var result = navigator.CreateFile(args[0], false, out var path);
        if(result is null) Log($"File created at {path}");
        else LogDefaultError(result.ToString());
    }

    private void DirectoryCommand(List<string> args)
    {
        if(!CheckArgumentCount(args.Count, 1)) return;
        var result = navigator.CreateFile(args[0], true, out var path);
        if(result is null) Log($"Directory created at {path}");
        else LogDefaultError(result.ToString());
    }

    private void TextCommand(List<string> args)
    {
        if(!CheckArgumentCount(args.Count, 1)) return;
        
        var window = Instantiate(Game.Current.WindowPrefab, Game.Current.Canvas.transform);
        var txt = Handle.ProcessWindow(this, Handle.GetProcess(ProcessId).Access.AccessLevel, window, "TextEditor") as TextEditorApplication;
        var path = navigator.GetModifiedPath(navigator.Path, args[0]);
        txt.FilePath = path;
        txt.WorkingDirectory = this.navigator.Path;
    }

    private void PermsCommand(List<string> args)
    {
        if(!CheckArgumentCount(args.Count, 0, 1)) return;

        var window = Instantiate(Game.Current.WindowPrefab, Game.Current.Canvas.transform);
        var perms = Handle.ProcessWindow(this, Handle.GetProcess(ProcessId).Access.AccessLevel, window, "Perms") as PermissionsApplication;
        var path = args.Count == 0 ? navigator.Path : navigator.GetModifiedPath(navigator.Path, args[0]);
        perms.TargetPath = path;
    }

    private void TreeCommand(List<string> args)
    {
        if(!CheckArgumentCount(args.Count, 0, 1)) return;
        var path = args.Count == 0 ? navigator.Path : args[0];
        var result = navigator.GetTree(path, out var tree);
        if(result is not null) { LogDefaultError(result.ToString()); return; }

        Debug.Log("reached");
        Debug.Log(tree.Value);
        Debug.Log(tree.Branches.Count);

        int indent = 0;
        void PrintOut(Computer.FileNavigator.TreeNode<string> focus)
        {
            Log(new string(' ', indent*2) + focus.Value.Split("/").Last());
            indent++;
            foreach(var branch in focus.Branches) PrintOut(branch);
            indent--;
        }

        PrintOut(tree);
    }

    public override WindowSettings GetSettings()
    {
        return new();
    }
}
