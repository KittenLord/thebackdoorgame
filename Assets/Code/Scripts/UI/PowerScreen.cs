using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable] public enum PowerType
{
    Power, Reboot, Sleep, Factory
}

public class PowerScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text InfoText;
    private PowerType type;
    [SerializeField] private GameObject ConfirmButton;

    void Start()
    {
        this.gameObject.SetActive(false);
    }

    public void OnPowerButton(int t)
    {
        this.gameObject.SetActive(true);
        ConfirmButton.SetActive(true);
        this.type = (PowerType)t;

        switch(type)
        {
            case PowerType.Power:
                InfoText.text = "POWER OFF: Turn off your computer and abandon the level. You will go to the main menu, and this level's progress will be lost";
                break;
            case PowerType.Reboot:
                InfoText.text = "RESTART: Restart your computer and reset the level's progress. You'll go back to the start of the level";
                break;
            case PowerType.Sleep:
                if(Game.Current.Stage.SleepFlags.All(s => Game.Current.Stage.GuideFlags.Contains(s)))
                {
                    InfoText.text = "SLEEP MODE: Put your computer into sleep mode. This will complete the level and make you proceed to the next one.";
                }
                else
                {
                    ConfirmButton.SetActive(false);
                    InfoText.text = "SLEEP MODE: Put your computer into sleep mode and complete the level. You haven't completed the objectives yet, so this is unavailable";
                }
                break;
            case PowerType.Factory:
                InfoText.text = "FACTORY RESET: If you have bricked your computer, you can factory reset your computer. It will delete your save, load default save, but your completed levels and achievements will be keptk";
                break;
        }
    }

    public void OnConfirm()
    {
        switch(type)
        {
            case PowerType.Power:
                SceneManager.LoadScene("MenuScene");
                break;
            case PowerType.Reboot:
                SceneManager.LoadScene("GameScene");
                break;
            case PowerType.Factory:
                // TODO implement this shit
                break;
            case PowerType.Sleep:
                Game.SelectedLevel++;
                if(Game.SelectedLevel >= Game.MaxLevel) { SceneManager.LoadScene("MenuScene"); }
                else { SceneManager.LoadScene("GameScene"); }
                break;
        }
        this.gameObject.SetActive(false);
    }

    public void GoBack()
    {
        this.gameObject.SetActive(false);
    }
}
