using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BootApplication : Application
{
    public override string Name => "boot";

    void Start()
    {
        // var window = Instantiate(Game.Current.WindowPrefab, Game.Current.Canvas.transform);
        // Handle.ProcessWindow(this, 5, window, "Terminal");

        var desktop = Instantiate(Application.Load("Desktop"), Game.Current.Canvas.transform);
        Handle.Authorize("admin", "123", 5, desktop);

        this.OnKilled();
    }
}