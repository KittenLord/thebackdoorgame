using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

// polymorphism for poor people (i cant be bothered to make it adequately)
public class LevelOperator
{
    public const string Trophy1 = "TROPHY1_REPLACE";
    public const string Trophy2 = "TROPHY2_REPLACE";
    public const string Trophy3 = "TROPHY3_REPLACE";

    public static string GenerateTrophy()
    {
        var pool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var a = new string(Enumerable.Range(0, 4).Select(_ => pool[UnityEngine.Random.Range(0, pool.Length)]).ToArray());
        var b = new string(Enumerable.Range(0, 4).Select(_ => pool[UnityEngine.Random.Range(0, pool.Length)]).ToArray());
        var c = new string(Enumerable.Range(0, 4).Select(_ => pool[UnityEngine.Random.Range(0, pool.Length)]).ToArray());

        return $"{a}-{b}-{c}";
    }

    public static string GenerateIp()
    {
        var ip = string.Join(".", Enumerable.Range(0, 4).Select(_ => UnityEngine.Random.Range(0, 256).ToString()).ToArray());
        return ip;
    }

    public static int GeneratePort()
    {
        return UnityEngine.Random.Range(1000, 65535);
    }

    public static string GeneratePassword()
    {
        string[] a = { 
            "banana", "dragon", "ball", "victory", "abrasiveness", "lol", "kek", "giant", 
            "vindiction", "explode", "cognition", "legato", "virgil", "ballistic", "bloodbath",
            "remedy", "call", "vitality", "dexterity", "fear", "terror", "globe", "sphere", "hyperboloid",
            "retention", "mosaic", "reluctance", "boycott", "cowboy", "bear", "wilderschweine", "log",
            "cumbersome", "chupacabra", "renegade", "globalwarming", "ouroboros", "death", "mantle",
            "gentle", "void", "abstractfactory", "comedy", "tragedy", "the" };
        return string.Join("_", a.OrderBy(_ => UnityEngine.Random.Range(0, 1000)).Take(3)) + UnityEngine.Random.Range(1000, 10000);
    }

    public static readonly Func<LevelOperator>[] List = new Func<LevelOperator>[] 
    {
        () => new LevelOperator {
            Initialize = (l) => {
                l.State[Trophy1] = GenerateTrophy();
            },
            ReplaceStage = (l, s) => {
                s = s.Replace(Trophy1, l.State[Trophy1]);
                return s;   
            },
            ReplaceComputer = (l, s) => {
                s = s.Replace(Trophy1, l.State[Trophy1]);
                return s;   
            }
        },
        () => new LevelOperator {
            Initialize = (l) => {
                l.State["MARIOIP_REPLACE"] = GenerateIp();
                l.State["MARIOPORT_REPLACE"] = GeneratePort().ToString();
                l.State["MARIOTROPHYPORT_REPLACE"] = GeneratePort().ToString();
                l.State["TARGETIP_REPLACE"] = GenerateIp();
                l.State["TARGETPASSWORD_REPLACE"] = GeneratePassword();
                Debug.Log(l.State["MARIOIP_REPLACE"]);
                Debug.Log(l.State["MARIOPORT_REPLACE"]);
                l.State[Trophy1] = GenerateTrophy();
                l.State[Trophy2] = GenerateTrophy();
            },
            ReplaceStage = (l, s) => {
                s = s.Replace("MARIOIP_REPLACE", l.State["MARIOIP_REPLACE"]);
                s = s.Replace("MARIOPORT_REPLACE", l.State["MARIOPORT_REPLACE"]);
                s = s.Replace("MARIOTROPHYPORT_REPLACE", l.State["MARIOTROPHYPORT_REPLACE"]);
                s = s.Replace("TARGETIP_REPLACE", l.State["TARGETIP_REPLACE"]);
                s = s.Replace("TARGETPASSWORD_REPLACE", l.State["TARGETPASSWORD_REPLACE"]);
                s = s.Replace(Trophy1, l.State[Trophy1]);
                s = s.Replace(Trophy2, l.State[Trophy2]);
                return s;
            },
            ReplaceComputer = (l, s) => {
                s = s.Replace("MARIOIP_REPLACE", l.State["MARIOIP_REPLACE"]);
                s = s.Replace("MARIOPORT_REPLACE", l.State["MARIOPORT_REPLACE"]);
                s = s.Replace("MARIOTROPHYPORT_REPLACE", l.State["MARIOTROPHYPORT_REPLACE"]);
                s = s.Replace(Trophy1, l.State[Trophy1]);
                s = s.Replace(Trophy2, l.State[Trophy2]);
                return s;
            },
            OnStartup = (l) => {
                var ip = l.State["MARIOIP_REPLACE"];   
                var computer = Game.Current.Stage.Computers.Find(c => c.Ip == ip);
                computer.Ports.Add(int.Parse(l.State["MARIOPORT_REPLACE"]), new File("mario-installer.exe", new(), false, true, "#EXECUTABLE_HEADER___\n___INSTALL___!!! mario\nsay #11FF11 \"Mario has been successfully installed!\"\n#"));
                computer.Ports.Add(int.Parse(l.State["MARIOTROPHYPORT_REPLACE"]), new File("trophy.txt", new(), false, false, $"good job\n{l.State[Trophy1]}"));
            }
        }
    };

    public Dictionary<string, string> State = new();
    public Action<LevelOperator> Initialize = (l) => {};
    public Func<LevelOperator, string, string> ReplaceStage = (l, s) => s;
    public Func<LevelOperator, string, string> ReplaceComputer = (l, s) => s;
    public Action<LevelOperator> OnStartup = l => {};
}