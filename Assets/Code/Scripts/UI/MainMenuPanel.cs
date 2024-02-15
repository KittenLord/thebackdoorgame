using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPanel : MonoBehaviour
{
    [field:SerializeField] public string Id { get; private set; }
    private float HiddenX;
    private float ShownX; 

    public void Show(float duration)
    {
        if(LastPositionCoroutine is not null) StopCoroutine(LastPositionCoroutine);
        LastPositionCoroutine = StartCoroutine(ChangePositionCoroutine(transform.position.x, ShownX * Screen.width, duration));
    }
    public void Hide(float duration)
    {
        if(LastPositionCoroutine is not null) StopCoroutine(LastPositionCoroutine);
        LastPositionCoroutine = StartCoroutine(ChangePositionCoroutine(transform.position.x, HiddenX * Screen.width, duration));
    }

    private Coroutine LastPositionCoroutine;
    private IEnumerator ChangePositionCoroutine(float start, float end, float duration)
    {
        float timer = 0.0f;
        while(timer <= 1.0f)
        {
            var lerp = Mathf.InverseLerp(0, duration, timer);
            var x = EasingFunction.EaseInOutCubic(start, end, lerp);
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            
            yield return null;
            timer += Time.deltaTime;
        }
    }

    void Start()
    {
        MainMenu.Main.Panels.Add(this);
        ShownX = (transform.position.x / Screen.width);
        HiddenX = (transform.position.x + 1300 * (Screen.width / 1920.0f)) / Screen.width;
        transform.position = new Vector3(HiddenX * Screen.width, transform.position.y, transform.position.z);
    }
}
