using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Application : MonoBehaviour
{
    public int ProcessId { get; set; }
    public abstract string Name { get; }
    public Computer.ComputerHandle Handle { get; set; }
    public virtual void OnKilled() { Handle.KillProcess(ProcessId); Destroy(this.gameObject); }

    public static Application Load(string name) => Load<Application>(name);
    public static T Load<T>(string name) where T : Application
    {
        string path = "Applications/" + name;
        Debug.Log(path);
        return Resources.Load<T>(path);
    }
}
