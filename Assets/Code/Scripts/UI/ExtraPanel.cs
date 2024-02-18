using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExtraPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text Trophies;

    void Start()
    {
        Trophies.text = $"You have collected {KEY.Trophies.Select(tr => SavedData.GetInt(tr)).Sum()} trophies out of 6 total";
        Trophies.text += $"\n{SavedData.GetInt(KEY.Trophies[0])} out of 1 trophies on level 1";
        Trophies.text += $"\n{SavedData.GetInt(KEY.Trophies[1]) + SavedData.GetInt(KEY.Trophies[2])} out of 2 trophies on level 2";
        Trophies.text += $"\n{SavedData.GetInt(KEY.Trophies[3]) + SavedData.GetInt(KEY.Trophies[4]) + SavedData.GetInt(KEY.Trophies[5])} out of 3 trophies on level 3";
    }

    public void OnErase()
    {
        SavedData.Reset();
        SceneManager.LoadScene("MenuScene");
    }
}
