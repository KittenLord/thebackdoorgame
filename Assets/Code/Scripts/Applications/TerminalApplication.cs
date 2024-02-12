using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TerminalApplication : WindowApplication
{
    [SerializeField] private TMP_Text ScrollText;
    [SerializeField] private Scrollbar Scrollbar;
    [SerializeField] private TMP_InputField Input;

    void Start()
    {
        Input.onSubmit.AddListener(ExecuteCommand);
    }

    private async void ExecuteCommand(string line)
    {
        string command = Input.text;
        var prefix = Handle.GetProcess(this.ProcessId).Access.AccessLevel + "$" + Handle.GetProcess(this.ProcessId).Access.Username;
        ScrollText.text += "\n" + $"<color=#AAAAAA>{prefix} ~/users/admin/ > </color>" + Input.text;
        Input.text = "";

        var args = CommandArgumentsParser.Parse(command);
        Debug.Log(string.Join("\n", args));

        if(args[0] == "cd") 
        {
            var nav = Handle.GetNavigator(ProcessId);
            var result = nav.Goto(args[1]);
            Debug.Log((result is not null ? result.ToString() : "success") + " " + nav.Path);
        }

        // Refocus on the input
        EventSystem.current.SetSelectedGameObject(Input.gameObject, null);
        Input.OnPointerClick(new PointerEventData(EventSystem.current));

        // Possibly add a coroutine
        await System.Threading.Tasks.Task.Delay(50);
        Scrollbar.value = 0;
    }

    public override WindowSettings GetSettings()
    {
        return new();
    }
}
