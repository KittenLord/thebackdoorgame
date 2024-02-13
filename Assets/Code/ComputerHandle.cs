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

        public bool VerifyUser(string username, string password)
        {
            return computer.Users.ContainsKey(username) && computer.Users[username].Password == password;
        }

        public Application ProcessWindow(Application parentProcess, int accessLevel, Window window, string application)
        {
            var app = window.LoadApplication(application);
            var result = computer.AddProcess(parentProcess.ProcessId, accessLevel, app);
            UnityEngine.Debug.Log(result);
            if(!result) { app.OnKilled(); window?.Close(); }
            app.Handle = this.Copy();
            return app;
        }

        public Application ProcessWindow(string username, string password, int accessLevel, Window window, string application)
        {
            var app = window.LoadApplication(application);
            var result = computer.AddProcess(username, password, accessLevel, app);
            if(!result) { app.OnKilled(); window?.Close(); }
            app.Handle = this.Copy();
            return app;
        }

        public void KillProcess(int pid)
        {
            //var process = GetProcess(pid);
            //process?.Application?.OnKilled();
            computer.Processes.Remove(pid);
        }

        public Dictionary<int, Process> GetAllProcesses() => computer.Processes;

        public Computer GetComputer() => computer;

        public bool UserExists(string user) => computer.Users.ContainsKey(user);

        public Process GetProcess(int pid) => computer.Processes.ContainsKey(pid) ? computer.Processes[pid] : null;
        public FileNavigator GetNavigator(int pid) => new FileNavigator(computer, computer.Processes[pid].Access);
        public string GetIp() => computer.Ip;
    }
}