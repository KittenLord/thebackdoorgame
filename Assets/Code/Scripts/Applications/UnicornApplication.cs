using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnicornApplication : WindowApplication
{
    public override string Name => "unicorn";

    [SerializeField] private Transform ContactsParent;
    [SerializeField] private Transform ResponsesParent;
    [SerializeField] private Transform MessagesParent;


    [SerializeField] private Transform TextMessageLeftPrefab;
    [SerializeField] private Transform TextMessageRightPrefab;
    [SerializeField] private Transform ImageMessageLeftPrefab;
    [SerializeField] private Transform ImageMessageRightPrefab;


    [SerializeField] private Transform ContactPrefab;
    [SerializeField] private Transform SeparatorPrefab;
    [SerializeField] private Transform ResponseOptionPrefab;


    private Dictionary<string, Image> contacts = new();


    public override WindowSettings GetSettings()
    {
        return new();
    }

    public string SelectedDialog;
    void Start()
    {
        UnicornApplication.ReceivedMessage += OnMessage;

        var dialogs = Game.Current.Stage.Dialogs;
        foreach(var dialog in dialogs)
        {
            var contact = Instantiate(ContactPrefab, ContactsParent);
            Instantiate(SeparatorPrefab, ContactsParent);
            contacts.Add(dialog.Username, contact.GetComponent<Image>());
            contact.GetChild(1).GetComponent<TMP_Text>().text = dialog.Username;
            var lastMessage = dialog.Messages.LastOrDefault();
            if(lastMessage is not null) contact.GetChild(2).GetComponent<TMP_Text>().text 
                = lastMessage.IsImage ? "<Image>" : lastMessage.Content;
        }

        var d = dialogs.FirstOrDefault();
        if(d is not null) SelectConversation(d.Username);
    }

    async void SelectConversation(string selectedDialog)
    {
        foreach(var c in contacts.Values) c.color = new Color(0, 0, 0);
        if(contacts.ContainsKey(selectedDialog)) contacts[selectedDialog].color = new Color(0.2f, 0.2f, 0.2f);

        var dialogs = Game.Current.Stage.Dialogs;
        var dialog = dialogs.FirstOrDefault();
        if(dialog is null) return;

        ClearSelection();

        foreach(var msg in dialog.Messages)
        {
            Transform tmsg = null;
            if(!msg.Self && !msg.IsImage) tmsg = Instantiate(TextMessageLeftPrefab, MessagesParent);
            else if(msg.Self && !msg.IsImage) tmsg = Instantiate(TextMessageRightPrefab, MessagesParent);
            else if(!msg.Self && msg.IsImage) tmsg = Instantiate(ImageMessageLeftPrefab, MessagesParent);
            else if(msg.Self && msg.IsImage) tmsg = Instantiate(ImageMessageRightPrefab, MessagesParent);

            if(!msg.IsImage) tmsg.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = msg.Content;
            else 
            {
                var img = tmsg.GetChild(0).GetChild(0).GetComponent<Image>();
                var sprite = LoadImage(msg.Content);
                img.sprite = sprite;
                var rect = img.GetComponent<RectTransform>();
                // dunno how to do this, will finish later
                // TODO
            }
        }

        var layout = MessagesParent.GetComponent<VerticalLayoutGroup>();
        layout.enabled = false;
        await System.Threading.Tasks.Task.Delay(5);
        layout.enabled = true;
    }

    Sprite LoadImage(string image)
    {
        return Resources.Load<Sprite>($"UnicornImages/{image}");
    }

    void ClearSelection()
    {
        foreach(Transform msg in MessagesParent) Destroy(msg.gameObject);
        foreach(Transform r in ResponsesParent) Destroy(r.gameObject);
    }

    void OnMessage(DialogMessage msg)
    {

    }

    public static event Action<DialogMessage> ReceivedMessage = (m) => {};
}
