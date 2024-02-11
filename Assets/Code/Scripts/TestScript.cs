using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Linq;

public class TestScript : MonoBehaviour
{
    public Window windowPrefab;
    public Canvas canvas;

    void Start()
    {
        var window = Instantiate(windowPrefab, canvas.transform);
        window.LoadApplication("Terminal", new());
    }
}
