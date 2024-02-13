using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DesktopApplication : Application
{
    public override string Name => "gui";

    private string Path;
    private Computer.FileNavigator navigator;
    private TerminalApplication Terminal;

    [SerializeField] private Transform IconsParent;
    [SerializeField] private Transform IconPrefab;

    void Start()
    {
        this.transform.SetAsFirstSibling();
        var process = Handle.GetProcess(ProcessId);
        if(process.Access.AccessLevel < 5) { this.OnKilled(); return; }

        var window = Instantiate(Game.Current.WindowPrefab, transform);
        var terminal = Handle.ProcessWindow(this, 5, window, "Terminal") as TerminalApplication;
        terminal.Window.GetComponent<RectTransform>().position = new Vector2(0, 9999);
        Terminal = terminal;

        navigator = Handle.GetNavigator(ProcessId);
        var user = process.Access.Username;
        Path = $"~/users/{user}/desktop";
        Reload();
    }

    async void Reload()
    {
        while(!Terminal.Initialized) await Task.Delay(50);
        var files = navigator.List(Path)
            .Where(f => f.EndsWith(".exe"));
        
        foreach(var path in files)
        {
            var icon = Instantiate(IconPrefab, IconsParent);
            var file = navigator.GetFile(path);
            var iconImageName = file.Contents.Split("\n").First(l => l.StartsWith("#icon")).Split(" ")[1];
            var fileName = path.Split("/").Last();

            icon.GetComponent<Button>().onClick.AddListener(() => {
                Terminal.ExecuteCommand($"run {path} new");
            });

            icon.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>($"DesktopIcons/{iconImageName}");
            icon.GetChild(1).GetComponent<TMP_Text>().text = fileName;
        }
    }

    void Update() { if(Terminal == null) this.OnKilled(); }
}