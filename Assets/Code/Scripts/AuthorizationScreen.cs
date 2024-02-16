using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthorizationScreen : MonoBehaviour
{
    [SerializeField] private TMP_InputField UsernameInput;
    [SerializeField] private TMP_InputField PasswordInput;
    [SerializeField] private TMP_Text ErrorText;
    [SerializeField] private Image SubmitButton;
    [SerializeField] private Transform LoadingIndicator;


    void Start()
    {
        ErrorText.gameObject.SetActive(false);
        LoadingIndicator.gameObject.SetActive(false);
    }

    public void OnSubmit()
    {
        ErrorText.text = "";
        if(UsernameInput.text.Any(c => char.IsWhiteSpace(c))) { ErrorText.text = "Your username cannot contain whitespace characters!"; return; }
        // check password (dont need to?)

        SubmitButton.gameObject.SetActive(false);
        LoadingIndicator.gameObject.SetActive(true);   

        StartCoroutine(Loading());     
    }

    private IEnumerator Loading()
    {
        float timer = 0;

        var outer = LoadingIndicator.GetChild(0);
        var inner = LoadingIndicator.GetChild(1);

        while(timer < 4)
        {
            outer.rotation *= Quaternion.Euler(0, 0, 140 * Time.deltaTime);
            inner.rotation *= Quaternion.Euler(0, 0, 220 * Time.deltaTime);
            yield return null;
            timer += Time.deltaTime;
        }

        var result = Game.Current.LoadComputer(UsernameInput.text, PasswordInput.text);
        if(result) 
        { 
            timer = 0;
            var cg = GetComponent<CanvasGroup>();
            while(timer < 1)
            {
                cg.alpha = 1 - timer;
                outer.rotation *= Quaternion.Euler(0, 0, 140 * Time.deltaTime);
                inner.rotation *= Quaternion.Euler(0, 0, 220 * Time.deltaTime);
                yield return null;
                timer += Time.deltaTime;
            }

            Destroy(this.gameObject); 
            yield break;
        }
        else
        {
            ErrorText.text = "Couldn't log into the account - password doesn't match.";
            SubmitButton.gameObject.SetActive(true);
            LoadingIndicator.gameObject.SetActive(false);
        }
    }
}