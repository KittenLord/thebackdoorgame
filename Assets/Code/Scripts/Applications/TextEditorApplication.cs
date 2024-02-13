using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class TextEditorApplication : WindowApplication
{
    public override string Name => "txt";
    public string FilePath { get; set; }
    public string WorkingDirectory { get; set; }
    public bool Readonly { get; set; }

    private Computer.FileNavigator navigator;
    [SerializeField] private TMP_InputField Text;

    void Start()
    {
        navigator = this.Handle.GetNavigator(ProcessId);
        var file = navigator.GetFile(FilePath);
        if(file is null) { Destroy(Window.gameObject); return; }

        if(file.IsObfuscated)
        {
            Readonly = true;
            Text.interactable = false;

            // TODO: make prettier, possibly like a hex editor?
            string palette = "awfnasp~!@#$%^& *()_+=`-/|\\";
            string text = "Binary data... " + new string(Enumerable.Range(0, 2000).Select(_ => palette[UnityEngine.Random.Range(0, palette.Length)]).ToArray());
            Text.text = text;

            return;
        }

        navigator.Goto(WorkingDirectory);
        var result = navigator.ReadFile(FilePath, out var contents);
        if(result is not null) 
        {
            Readonly = true;
            Text.interactable = false;
            Text.textComponent.color = new Color(1, 0, 0, 1);
            Text.text = result.ToString();
            return;
        }

        if(!navigator.CanWriteToFile(FilePath)) { Readonly = true; Text.interactable = false; }
        Text.text = contents;
    }

    public void SaveButton()
    {
        if(Readonly) return;
        if(navigator.GetFile(FilePath).IsObfuscated) return;
        var result = navigator.WriteFile(FilePath, Text.text);
        if(result is not null)
        {
            Readonly = true;
            Text.interactable = false;
            Text.textComponent.color = new Color(1, 0, 0, 1);
            Text.text = result.ToString();
            return;
        }
    }

    public override WindowSettings GetSettings()
    {
        return new WindowSettings{ BackgroundColor = new Color(0.7f, 0.7f, 0.7f, 1) };
    }
}