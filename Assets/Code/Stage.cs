using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Stage
{
    public List<Computer> Computers { get; private set; } = new();
    public List<List<string>> Networks { get; private set; } = new();
    public List<DialogHistory> Dialogs { get; private set; } = new();


    [JsonIgnore] public List<string> GuideFlags = new();
    [JsonIgnore] public List<object> GuideStates = new();
}