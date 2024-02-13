using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public abstract class WindowApplication : Application
{
    public Window Window { get; set; }
    public abstract WindowSettings GetSettings();

    public override void OnKilled() { Handle.KillProcess(ProcessId); Window.Close(); }
    public void OnClosed() { OnKilled(); }
}