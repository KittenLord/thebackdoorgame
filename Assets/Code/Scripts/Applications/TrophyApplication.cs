using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class TrophyApplication : WindowApplication
{
    [SerializeField] private TMP_InputField Input;
    [SerializeField] private TMP_Text ResultText;

    void Start()
    {
        ResultText.gameObject.SetActive(false);
    }

    public void OnSubmit()
    {
        ResultText.gameObject.SetActive(false);
        var trophy = Input.text;
        Input.text = "";

        var stage = Game.Current.Stage;
        if(!stage.Trophies.ContainsKey(trophy)) 
        { 
            ResultText.gameObject.SetActive(true); 
            ResultText.color = new Color(1, 0.1f, 0.1f); 
            ResultText.text = "Invalid trophy token!"; 
            return; 
        }

        var trophyId = stage.Trophies[trophy];
        if(stage.GuideFlags.Contains(trophyId))
        {
            ResultText.gameObject.SetActive(true); 
            ResultText.color = new Color(1, 1, 0.1f); 
            ResultText.text = "This trophy has already been claimed!"; 
            return;
        }

        ResultText.gameObject.SetActive(true); 
        ResultText.color = new Color(0.1f, 1, 0.1f); 
        ResultText.text = "You have successfully claimed this trophy!"; 
        SavedData.SetFlag(trophyId);
        stage.GuideFlags.Add(trophyId);
    }

    public override string Name => "trophypp";
    public override WindowSettings GetSettings()
    {
        return new() { BackgroundColor = new Color(0.52f, 0.42f, 0.38f) };
    }



}