using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;

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
        }
    };

    public Dictionary<string, string> State = new();
    public Action<LevelOperator> Initialize = (l) => {};
    public Func<LevelOperator, string, string> ReplaceStage = (l, s) => s;
    public Func<LevelOperator, string, string> ReplaceComputer = (l, s) => s;
}