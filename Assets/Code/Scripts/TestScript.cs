using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Linq;

public class TestScript : MonoBehaviour
{
    void Start()
    {
        var computer = JsonConvert.DeserializeObject<Computer>(Resources.Load<TextAsset>("Testing/testcomputer").text);
        var boot = Instantiate(Application.Load("Boot"), Game.Current.Canvas.transform);
        var handle = new Computer.ComputerHandle(computer);
        var result = handle.Authorize("admin", "123", 5, boot);
        Debug.Log(result);
    }
}
