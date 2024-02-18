using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AmethystDownloader : MonoBehaviour
{   
    private Computer amethystComputer;
    private TerminalApplication terminal;
    private string giraffeqlIp;

    void Start()
    {
        amethystComputer = Game.Current.Stage.Computers.Find(c => c.Users.ContainsKey("amethyst") && c != Game.Current.computer);

        var giraffeqlComputer = Game.Current.Stage.Computers.Find(c => c.Users.ContainsKey("amethyst_giraffeql") && c != Game.Current.computer);
        giraffeqlIp = giraffeqlComputer.Ip;
        giraffeqlComputer.Ports.Add(80, new File("sync.exe", new(), false, true, "#EXECUTABLE_HEADER___"));

        var handle = new Computer.ComputerHandle(amethystComputer);
        var window = Instantiate(Game.Current.WindowPrefab, Game.Current.transform);
        window.transform.position += new Vector3(0, 1, 0) * 20000;
        terminal = handle.ProcessWindow("root", "123", 0, window, "Terminal") as TerminalApplication;
        terminal.DontFocus = true;
        terminal.IgnoreRules = true;
    }

    void Update()
    {
        if(Time.frameCount % 500 != 0) return;
        if(terminal == null) { if(!Game.Current.Stage.GuideFlags.Contains("amethyst")) Game.Current.Stage.GuideFlags.Add("amethyst"); return; }
        StartCoroutine(DownloadCoroutine());
    }

    IEnumerator DownloadCoroutine()
    {
        terminal.ExecuteCommand($"del ~/sync.exe");
        yield return new WaitForSeconds(0.5f);
        terminal.ExecuteCommand($"port download {giraffeqlIp} 80");
        yield return new WaitForSeconds(0.5f);
        terminal.ExecuteCommand($"run ~/sync.exe");
    }
}
