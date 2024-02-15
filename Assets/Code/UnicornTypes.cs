using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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