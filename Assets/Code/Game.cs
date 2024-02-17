using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public const int MaxLevel = 3;

    public static Game Current { get; private set; }
    public static int SelectedLevel { get; set; }

    public Stage Stage { get; private set; }
    [field:SerializeField] public Canvas Canvas { get; private set; }
    [field:SerializeField] public Window WindowPrefab { get; private set; }

    public const string GuideUsername = "computer_guy";

    public int UserResponse = -1;
    public List<string> UserOptions = new();

    public LevelOperator Operator;

    public Computer computer { get; private set; }

    void Awake() { Current = this; }
    void Start() 
    {
        if(SelectedLevel < 0 || SelectedLevel >= MaxLevel) { Debug.Log($"EXIT: bad level # {SelectedLevel}"); SceneManager.LoadScene("MenuScene"); return; }
        int index = SelectedLevel + 1;

        Operator = LevelOperator.List[SelectedLevel]();
        Operator.Initialize(Operator);

        var stagePath = $"ComputerData/Stages/level{index}stage";
        var stage = JsonConvert.DeserializeObject<Stage>(Operator.ReplaceStage(Operator, Resources.Load<TextAsset>(stagePath).text));
        this.Stage = stage;

    }

    // void Update() { if(Input.GetKeyDown(KeyCode.RightBracket)) Debug.Log(JsonConvert.SerializeObject(computer)); }

    private void OnMessageSfx(string s, bool n)
    {
        { if(n) Sound.Main.Play("SFX/message", Sound.Tag.SFX, true); };
    }

    void OnDestroy() 
    {
        UnicornApplication.ReceivedMessage -= OnMessageSfx;
    }

    private IEnumerator GuideStateMachineRunner()
    {
        var states = Stage.GuideStates;
        UnicornApplication.ReceivedMessage += OnMessageSfx;

        DialogState GetState(string id) { Debug.Log(id); return states.Find(s => s.Id == id); }

        var state = GetState("_");

        while(state.Type != DialogStateType.End)
        {
            yield return new WaitForSeconds(0.2f);
            switch(state.Type)
            {
                case DialogStateType.Flag:
                    Game.Current.Stage.GuideFlags.Add(state.Args[1]);
                    state = GetState(state.Args[0]);
                    break;
                case DialogStateType.Goto:
                    state = GetState(state.Args[0]);
                    break;
                case DialogStateType.Delay:
                    yield return new WaitForSeconds(float.Parse(state.Args[1]));
                    state = GetState(state.Args[0]);
                    break;
                case DialogStateType.Message:
                    // TODO typing indicator?
                    yield return new WaitForSeconds(state.Args[1].Length / 20.0f);

                    Stage.Dialogs.Find(d => d.Username == Game.GuideUsername).Messages.Add(new DialogMessage(state.Args[1], false, false));
                    UnicornApplication.ReceiveMessage(Game.GuideUsername, true);
                    state = GetState(state.Args[0]);
                    break;
                case DialogStateType.Image:
                    Stage.Dialogs.Find(d => d.Username == Game.GuideUsername).Messages.Add(new DialogMessage(state.Args[1], true, false));
                    UnicornApplication.ReceiveMessage(Game.GuideUsername, true);
                    state = GetState(state.Args[0]);
                    break;
                case DialogStateType.Options:

                    var options = state.Args.Select(arg => {
                        var split = arg.Split("`");
                        return (text: split[0], next: split[1], flags: split[2].Split("|").Where(f => f != "").ToList());
                    }).ToList();

                    while(UserResponse == -1) 
                    {
                        var userOptions = options.Select(option => option.flags.All(Game.Current.Stage.GuideFlags.Contains) ? option.text : "").ToList();
                        if(!userOptions.SequenceEqual(Game.Current.UserOptions)) 
                        {
                            Game.Current.UserOptions = userOptions;
                            UnicornApplication.ReceiveMessage(Game.GuideUsername, false);
                        }

                        yield return new WaitForSeconds(2f);
                    }
                    Game.Current.UserOptions = new();
                    //UnicornApplication.ReceiveMessage(Game.GuideUsername, false);

                    var r = UserResponse;
                    UserResponse = -1;
                    Debug.Log(r);

                    state = GetState(options[r].next);
                    break;
                case DialogStateType.WaitUntil:
                    var flags = state.Args[1].Split("|");
                    while(!flags.All(f => Game.Current.Stage.GuideFlags.Contains(f))) yield return new WaitForSeconds(5f);
                    state = GetState(state.Args[0]);
                    break;
            }
        }
    }

    public bool LoadComputer(string username, string password)
    {
        var index = SelectedLevel + 1;
        if(SavedData.IsSaved($"level{index}computer")) 
        {
            Debug.Log("Loading saved computer");
            var c = SavedData.GetJson<Computer>($"level{index}computer", (s) => Operator.ReplaceComputer(Operator, s));
            if(new Computer.ComputerHandle(c).VerifyUser(username, password)) 
            { computer = c; }
            else { return false; }
        }
        else 
        {
            if((SavedData.IsSaved(KEY.SavedUsername) && SavedData.IsSaved(KEY.SavedPassword)) &&
               (SavedData.GetString(KEY.SavedUsername) != username || SavedData.GetString(KEY.SavedPassword) != password)) return false;

            SavedData.SetString(KEY.SavedUsername, username);
            SavedData.SetString(KEY.SavedPassword, password);

            Debug.Log("Loading default computer");
            var computerPath = $"ComputerData/DefaultPlayerComputers/level{index}computer";
            var json = Resources.Load<TextAsset>(computerPath).text;
            json = json.Replace("USERNAME_REPLACE", username);
            json = json.Replace("PASSWORD_REPLACE", password);
            computer = JsonConvert.DeserializeObject<Computer>(Operator.ReplaceComputer(Operator, json));
        }

        SavedData.SetFlag(KEY.SaveExists);
        var desktop = Instantiate(Application.Load("Desktop"), Game.Current.Canvas.transform);
        desktop.transform.SetAsFirstSibling();
        var handle = new Computer.ComputerHandle(computer);
        handle.Authorize(username, password, 5, desktop);

        // only start after computer starts, so the login screen time doesnt count
        StartCoroutine(GuideStateMachineRunner());
        Operator.OnStartup(Operator);

        return true;
    }
}