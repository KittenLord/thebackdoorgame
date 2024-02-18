using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreen : MonoBehaviour
{
    public void OnClick()
    {
        StartCoroutine(Animation());
    }

    private IEnumerator Animation()
    {
        var cg = GetComponent<CanvasGroup>();
        float timer = 0;
        while(timer < 3)
        {
            timer += Time.deltaTime;
            cg.alpha = 1 - (timer / 3);
            yield return null;
        }
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
