using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

public class DialogMessage
{
    public string Content;
    public bool IsImage;
    public bool Self;

    public DialogMessage(string content, bool isImage, bool self)
    {
        Content = content;
        IsImage = isImage;
        Self = self;
    }
}

public class DialogHistory
{
    public string Username;
    public List<DialogMessage> Messages;

    public DialogHistory(string username, List<DialogMessage> messages)
    {
        Username = username;
        Messages = messages;
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum DialogStateType
{
    Delay, Message, Image, Options, End, WaitUntil, Goto, Flag
}

public class DialogState
{
    public string Id;
    public DialogStateType Type;
    public List<string> Args;

    public DialogState(string id, DialogStateType type, List<string> args)
    {
        Id = id;
        Type = type;
        Args = args;
    }
}