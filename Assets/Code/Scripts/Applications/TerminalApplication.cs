using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using Unity.VisualScripting;
using TreeEditor;
using UnityEditor.Compilation;

public class TerminalApplication : WindowApplication
{
    [SerializeField] private TMP_Text ScrollText;
    [SerializeField] private Scrollbar Scrollbar;
    [SerializeField] private TMP_InputField Input;

    private Computer.FileNavigator navigator;

    private bool Initialized = false;
    private bool StealInput = false;
    private System.Action<string> CustomInputHandler;

    void Start()
    {
        Window.SetTitle($"Terminal @ {Handle.GetIp()}");
        Input.onSubmit.AddListener(ExecuteCommand);
        navigator = Handle.GetNavigator(ProcessId);
        if(navigator.Goto($"~/users/{Handle.GetProcess(ProcessId).Access.Username}") is not null) navigator.Goto("~");
        Initialized = true;
    }

    public int LastId { get; private set; }
    private void Respond() => LastId = Random.Range(int.MinValue, int.MaxValue);

    private void ExecuteCommand(string line)
    {
        if(StealInput) { CustomInputHandler?.Invoke(line); return; }

        string command = line;
        var prefix = Handle.GetProcess(this.ProcessId).Access.AccessLevel + "$" + Handle.GetProcess(this.ProcessId).Access.Username;
        Log($"<color=#AAAAAA>{prefix} {navigator.Path} > </color>" + line);
        Input.text = "";

        if(command.StartsWith("#")) { Respond(); return; }

        var args = CommandArgumentsParser.Parse(command);
        if(args.Count <= 0) { Respond(); return; }
        var first = args.First();
        args.RemoveAt(0);

        if(!navigator.FileExists($"~/system/installed/{first}.exe"))
        {
            LogError($"\"{first}\" is not recognized as a command/executable. Are you sure it is installed?");
            Respond();
            return;
        }

        if(first == "cd") CdCommand(args);
        if(first == "ls") LsCommand(args);
        if(first == "file") FileCommand(args);
        if(first == "dir") DirectoryCommand(args);
        if(first == "txt") TextCommand(args);
        if(first == "perms") PermsCommand(args);
        if(first == "tree") TreeCommand(args);
        if(first == "auth") AuthCommand(args);
        if(first == "permslist") PermsListCommand(args);
        if(first == "terminal") TerminalCommand(args);
        if(first == "kill") KillCommand(args);
        if(first == "tms") TmsCommand(args);
        if(first == "run") RunCommand(args);

        Respond();
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
    private void LogArgumentError(int argument, string expected, string actual) => 
        LogError($"An argument error occurred, at position {argument}\nExpected: {expected}\nFound: {actual}");

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

        var sourcePath = args.Count == 0 ? navigator.Path : args[0];
        sourcePath = navigator.GetModifiedPath(navigator.Path, sourcePath);
        if(sourcePath is null) { LogDefaultError("Invalid path"); return; }

        var result = navigator.List(sourcePath);
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
        
        Debug.Log("TXT REACHED");
        var window = Instantiate(Game.Current.WindowPrefab, Game.Current.Canvas.transform);
        var txt = Handle.ProcessWindow(this, Handle.GetProcess(ProcessId).Access.AccessLevel, window, "TextEditor") as TextEditorApplication;
        var path = navigator.GetModifiedPath(navigator.Path, args[0]);
        txt.FilePath = path;
        txt.WorkingDirectory = this.navigator.Path;
        Debug.Log("PATH ERROR");
        Debug.Log(navigator is null);
        Debug.Log(navigator.Path);
        Debug.Log(args[0]);
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

    private void AuthCommand(List<string> args)
    {
        if(!CheckArgumentCount(args.Count, 1)) return;
        if(!Handle.UserExists(args[0])) { LogDefaultError($"User with username {args[0]} does not exist."); return; }
        
        LogRaw($"\nInsert password for user {args[0]}: ");

        StealInput = true;
        Input.inputType = TMP_InputField.InputType.Password;
        CustomInputHandler = async password => {

            LogRaw(new string('*', password.Length));
            Input.text = "";
            Input.interactable = false;
            Input.inputType = TMP_InputField.InputType.Standard;

            await System.Threading.Tasks.Task.Delay(2000);

            StealInput = false;
            Input.interactable = true;

            var verify = Handle.VerifyUser(args[0], password);
            if(!verify) { LogError($"Incorrect password for user {args[0]}"); return; }

            var window = Instantiate(Game.Current.WindowPrefab, Game.Current.Canvas.transform);
            Handle.ProcessWindow(args[0], password, 0, window, "Terminal");
            this.OnKilled();
        };
    }

    private void PermsListCommand(List<string> args)
    {
        if(!CheckArgumentCount(args.Count, 0, 1)) return;
        var path = args.Count == 0 ? navigator.Path : navigator.GetModifiedPath(navigator.Path, args[0]);
        var result = navigator.GetFilePermissions(path, out var permissions);
        if(result is not null) { LogDefaultError(result.ToString()); return; }

        if(!permissions.Fit(this.Handle.GetProcess(ProcessId).Access, FilePermission.Inspect)) { LogDefaultError("Can't access this file"); return; }
        var categories = System.Enum.GetValues(typeof(FilePermission)).Cast<FilePermission>().ToList();

        foreach(var category in categories) Log($"{category.ToString().ToUpper()}: {string.Join(", ", permissions[category])}");
    }

    private void TerminalCommand(List<string> args)
    {
        if(!CheckArgumentCount(args.Count, 0)) return;

        var window = Instantiate(Game.Current.WindowPrefab, Game.Current.Canvas.transform);
        Handle.ProcessWindow(this, 0, window, "Terminal");
    }

    private void KillCommand(List<string> args)
    {
        if(!CheckArgumentCount(args.Count, 0)) return;
        this.OnKilled();
    }

    private void TmsCommand(List<string> args)
    {
        if(!CheckArgumentCount(args.Count, 1)) return;
        var sourcePath = navigator.GetModifiedPath(navigator.Path, args[0]);
        
        var result = navigator.GetFilePermissions(sourcePath, out var p);
        if(result is not null) { LogDefaultError(result.ToString()); return; }

        if(!p.Fit(Handle.GetProcess(ProcessId).Access, FilePermission.Inspect, FilePermission.Read)) { LogDefaultError("Forbidden."); return; }

        var file = navigator.GetFile(sourcePath);

        var split = sourcePath.Split("/");
        var pathPrefix = string.Join("/", split.Take(split.Length - 1));

        var fileName = split.Last();
        var fsplit = fileName.Split(".");
        var pathPostfix = string.Join(".", fsplit.Take(fsplit.Length <= 1 ? 1 : fsplit.Length - 1));
        
        var destinationPath = pathPrefix + "/" + pathPostfix + ".exe";
        if(navigator.GetFile(destinationPath) is not null) { LogError($"The file at path \"{destinationPath}\" already exists."); return; }

        result = navigator.CreateFile(destinationPath, false, out var createdPath);
        if(result is not null) { LogDefaultError(result.ToString()); return; }

        var outputFile = navigator.GetFile(createdPath);
        outputFile.Contents = "#EXECUTABLE_HEADER___" + "\n" + file.Contents;
        outputFile.IsObfuscated = true;
    }

    private async void RunCommand(List<string> args)
    {
        var path = navigator.GetModifiedPath(navigator.Path, args[0]);
        if(path is null) { LogArgumentError(1, "path", $"\"{path}\""); return; }
        if(!path.EndsWith(".exe")) { LogError("You are trying to run a non-executable file."); return; }
        
        var result = navigator.GetFilePermissions(path, out var p);
        if(result is not null) { LogDefaultError(result.ToString()); return; }
        if(!p.Fit(Handle.GetProcess(ProcessId).Access, FilePermission.Inspect, FilePermission.Read)) { LogDefaultError("Forbidden."); return; }

        string scriptAddendum = "";
        if(args.Count > 2) { scriptAddendum = string.Join("", args.TakeLast(args.Count - 2).Select(s => " " + s)); }
        scriptAddendum = scriptAddendum.Replace("\n", "").Replace("\r", ""); // prevent from injection

        var script = (navigator.GetFile(path).Contents + scriptAddendum).Split("\n").Select(s => s.Trim()).ToList();
        if(script.FirstOrDefault() != "#EXECUTABLE_HEADER___") { LogError("Fatal error: executable's signature is invalid."); return; }

        var mode = args.Count >= 2 ? args[1] : "new";

        if(mode == "new")
        {
            var window = Instantiate(Game.Current.WindowPrefab, Game.Current.Canvas.transform);
            var terminal = Handle.ProcessWindow(this, this.Handle.GetProcess(ProcessId).Access.AccessLevel, window, "Terminal") as TerminalApplication;

            while (!terminal.Initialized) await System.Threading.Tasks.Task.Delay(50);

            foreach (var line in script)
            {
                if(line.StartsWith("#")) continue;
                var id = terminal.LastId;
                terminal.ExecuteCommand(line);
                while (terminal.LastId == id) await System.Threading.Tasks.Task.Delay(50);

                //await System.Threading.Tasks.Task.Delay(5000);
            }

            terminal.OnKilled();
        }
        else if(mode == "this")
        {
            foreach (var line in script)
            {
                if(line.StartsWith("#")) continue;
                Input.interactable = false;
                var id = this.LastId;
                this.ExecuteCommand(line);
                while (this.LastId == id) await System.Threading.Tasks.Task.Delay(50);

                //await System.Threading.Tasks.Task.Delay(5000);
                Input.interactable = false;
            }
            Input.interactable = true;
        }
        else
        {
            LogArgumentError(2, "[new|this]", mode);
            return;
        }
    }

    

    public override WindowSettings GetSettings()
    {
        return new();
    }
}
