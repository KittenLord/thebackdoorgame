using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Stage
{
    public static Stage Current { get; set; }
    public List<Computer> Computers { get; private set; } = new();
    public List<List<string>> Networks { get; private set; } = new();
}