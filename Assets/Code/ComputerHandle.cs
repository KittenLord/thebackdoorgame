using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public partial class Computer
{
    public class ComputerHandle
    {
        private Computer computer;
        public ComputerHandle(Computer computer) { this.computer = computer; }
        public ComputerHandle Copy() => new ComputerHandle(computer);

        public bool Authorize(string username, string password, int accessLevel, Application application)
        {
            var result = computer.AddProcess(username, password, accessLevel, application);
            if(result) application.Handle = this.Copy();
            return result;
        }

        public void ProcessWindow(Application parentProcess, int accessLevel, Window window, string application)
        {
            var app = window.LoadApplication(application);
            var result = computer.AddProcess(parentProcess.ProcessId, accessLevel, app);
            UnityEngine.Debug.Log(result);
            if(!result) { app.OnKilled(); window?.Close(); }
            app.Handle = this.Copy();
        }

        public Process GetProcess(int pid) => computer.Processes.ContainsKey(pid) ? computer.Processes[pid] : null;
    }
}