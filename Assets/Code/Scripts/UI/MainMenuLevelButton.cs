using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuLevelButton : MonoBehaviour
{
    void Start()
    {
        transform.GetChild(0).GetComponent<TMP_Text>().text = (transform.GetSiblingIndex() + 1).ToString();
        this.GetComponent<Button>().interactable = transform.GetSiblingIndex() == 0 || SavedData.GetFlag($"level{transform.GetSiblingIndex()}");
    }

    void Update()
    {
        
    }
}
