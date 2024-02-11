using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BootApplication : Application
{
    void Start()
    {
        var window = Instantiate(Game.Current.WindowPrefab, Game.Current.Canvas.transform);
        Handle.ProcessWindow(this, 2, window, "Terminal");
        Destroy(this.gameObject);
    }
}