using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Stage
{
    public List<Computer> Computers { get; private set; } = new();
    public List<string> SleepFlags { get; private set; } = new();
    public Dictionary<string, string> Trophies { get; private set; } = new();
    public List<DialogHistory> Dialogs { get; private set; } = new();
    public List<DialogState> GuideStates { get; private set; } = new();


    [JsonIgnore] public List<string> GuideFlags = new();
}