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


    [SerializeField] private Scrollbar MessageScrollBar;


    private Dictionary<string, Image> contacts = new();


    public override WindowSettings GetSettings()
    {
        return new();
    }

    public string SelectedDialog;
    void Start()
    {
        Window.SetTitle("Unicorn Client");
        UnicornApplication.ReceivedMessage += OnMessage;

        var dialogs = Game.Current.Stage.Dialogs;
        foreach(var dialog in dialogs)
        {
            var contact = Instantiate(ContactPrefab, ContactsParent);
            string id = dialog.Username;
            contact.GetComponent<Button>().onClick.AddListener(() => { SelectConversation(id); });
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

    public void OnMessage(string username, bool newMessage)
    {
        if(SelectedDialog != username) return;
        if(newMessage) Sound.Main.Play("SFX/message", Sound.Tag.SFX, true);
        SelectConversation(username);
    }

    void OnDestroy()
    {
        ReceivedMessage -= OnMessage;
    }

    public static event Action<string, bool> ReceivedMessage = (s, n) => {};
    public static void ReceiveMessage(string s, bool newMessage) => ReceivedMessage.Invoke(s, newMessage);

    int D = 0;
    void SelectConversation(string selectedDialog)
    {
        try{
        Debug.Log(D++);

        var oldSelectedDialog = SelectedDialog;
        SelectedDialog = selectedDialog;
        ClearSelection();
        foreach(var c in contacts.Values) c.color = new Color(1, 1, 1);
        if(contacts.ContainsKey(selectedDialog)) contacts[selectedDialog].color = new Color(0.8f, 0.8f, 0.8f);

        var dialogs = Game.Current.Stage.Dialogs;
        var dialog = dialogs.FirstOrDefault(d => d.Username == selectedDialog);
        if(dialog is null) dialog = dialogs.FirstOrDefault();
        if(dialog is null) return;


        for(int i = 0; i < Game.Current.UserOptions.Count; i++)
        {
            if(SelectedDialog != Game.GuideUsername) break;
            int n = i; 
            var option = Game.Current.UserOptions[i];
            if(option == "") continue;
            var r = Instantiate(ResponseOptionPrefab, ResponsesParent);
            r.GetChild(0).GetComponent<TMP_Text>().text = option;
            r.GetComponent<Button>().onClick.AddListener(async () => { 
                Game.Current.UserResponse = n; 
                Game.Current.Stage.Dialogs.Find(d => d.Username == Game.GuideUsername).Messages.Add(new DialogMessage(option, false, true));
                Game.Current.UserOptions = new();
                foreach(Transform o in ResponsesParent) Destroy(o.gameObject);
                UnicornApplication.ReceiveMessage(Game.GuideUsername, false);
                await System.Threading.Tasks.Task.Delay(100);
                ReloadLayouts();
            });
        }

        //ReloadLayouts();

        var messages = new List<DialogMessage>(dialog.Messages);
        foreach(var msg in messages)
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

                while(rect.rect.width < 400) { rect.sizeDelta *= 1.2f; }
            }
        }

        ReloadLayouts();

        if(oldSelectedDialog != SelectedDialog) MessageScrollBar.value = 0;
        }catch(Exception e){Debug.Log(e);}
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

    async void ReloadLayouts()
    {
        var layout = MessagesParent.GetComponent<VerticalLayoutGroup>();
        var l = ResponsesParent.GetComponent<HorizontalLayoutGroup>();
        layout.enabled = false;
        l.enabled = false;

        await System.Threading.Tasks.Task.Delay(5);

        layout.enabled = true;
        l.enabled = true;
    }
}
