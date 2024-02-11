using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Computer
{
    public class FileNavigator
    {
        private Computer computer;
        public FileNavigator(Computer computer) { this.computer = computer; }

        public string Path { get; private set; }
    }
}