using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Application : MonoBehaviour
{
    public Computer Computer { get; private set; }
    public Window Window { get; private set; }
    public void SetContext(Computer computer, Window window)
    {
        Computer = computer;
        Window = window;
    }

    public abstract WindowSettings GetSettings();

    public virtual void OnClosed() { Window.Close(); }
    public virtual void OnKilled() { Window.Close(); }
}
