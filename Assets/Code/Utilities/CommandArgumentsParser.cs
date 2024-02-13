using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class CommandArgumentsParser
{
    private enum State
    {
        Normal, String, StringEscape
    }

    public static List<string> Parse(string command)
    {
        List<string> result = new List<string>();
        string buffer = "";
        State state = State.Normal;
        for(int i = 0; i < command.Length; i++)
        {
            string next = command[i].ToString();
            switch(state)
            {
                case State.Normal:
                    if(next == "\"" || next == "\'") { state = State.String; continue; }
                    if(next == " " && buffer == "") continue;
                    if(next == " ") { result.Add(buffer); buffer = ""; continue; }
                    buffer += next;
                    break;
                case State.String:
                    if(next == "\"" || next == "\'") { state = State.Normal; continue; }
                    if(next == "\\") { state = State.StringEscape; continue; }
                    buffer += next;
                    break;
                case State.StringEscape:
                    if(next == "n") next = "\n";
                    if(next == "\\") next = "\\";
                    // ... 
                    buffer += next;
                    state = State.String;
                    break;
            }
        }
        if(buffer != "") result.Add(buffer);
        return result;
    } 
}