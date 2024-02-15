using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static Game Current { get; private set; }
    public static int SelectedLevel { get; set; }

    public Stage Stage { get; private set; }
    [field:SerializeField] public Canvas Canvas { get; private set; }
    [field:SerializeField] public Window WindowPrefab { get; private set; }

    public Computer computer { get; private set; }

    void Awake() { Current = this; }
    void Start() 
    {
        if(SelectedLevel < 0 || SelectedLevel > 20) { Debug.Log($"EXIT: bad level # {SelectedLevel}"); SceneManager.LoadScene("MenuScene"); return; }
        int index = SelectedLevel + 1;

        var stagePath = $"ComputerData/Stages/level{index}stage";
        var stage = JsonConvert.DeserializeObject<Stage>(Resources.Load<TextAsset>(stagePath).text);
        this.Stage = stage;
    }

    public bool LoadComputer(string username, string password)
    {
        var index = SelectedLevel + 1;
        if(SavedData.IsSaved($"level{index}computer")) 
        {
            Debug.Log("Loading saved computer");
            var c = SavedData.GetJson<Computer>($"level{index}computer");
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
            computer = JsonConvert.DeserializeObject<Computer>(json);
        }

        SavedData.SetFlag(KEY.SaveExists);
        var desktop = Instantiate(Application.Load("Desktop"), Game.Current.Canvas.transform);
        desktop.transform.SetAsFirstSibling();
        var handle = new Computer.ComputerHandle(computer);
        handle.Authorize(username, password, 5, desktop);
        return true;
    }
}