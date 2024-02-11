using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TerminalApplication : Application
{
    [SerializeField] private TMP_Text ScrollText;
    [SerializeField] private Scrollbar Scrollbar;
    [SerializeField] private TMP_InputField Input;

    void Start()
    {
        Input.onSubmit.AddListener(OnEnter);
    }

    private async void OnEnter(string line)
    {
        string command = Input.text;
        ScrollText.text += "\n" + "<color=#AAAAAA>$user > </color>" + Input.text;
        Input.text = "";

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
