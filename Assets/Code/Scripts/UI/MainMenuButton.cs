using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image ColorSprite;
    [SerializeField] private Transform MoveTransform;

    public void ChangeColor(Color color, float duration)
    {
        if(LastColorCoroutine is not null) StopCoroutine(LastColorCoroutine);
        LastColorCoroutine = StartCoroutine(ChangeColorCoroutine(ColorSprite.color, color, duration));
    }

    private Coroutine LastColorCoroutine;
    private IEnumerator ChangeColorCoroutine(Color start, Color end, float duration)
    {
        float timer = 0.0f;
        while(timer <= 1.0f)
        {
            var lerp = Mathf.InverseLerp(0, duration, timer);
            ColorSprite.color = UnityEngine.Color.Lerp(start, end, lerp);
            yield return null;
            timer += Time.deltaTime;
        }
    }

    public static Color DefaultColor => new Color(0.33f, 0.68f, 0.72f);
    public static Color HoverColor => new Color(0.16f, 0.8f, 0.8f);
    public static Color SelectedColor => new Color(0, 1, 1);


    public void ChangePosition(float x, float duration)
    {
        if(LastPositionCoroutine is not null) StopCoroutine(LastPositionCoroutine);
        LastPositionCoroutine = StartCoroutine(ChangePositionCoroutine(MoveTransform.position.x, x, duration));
    }

    private Coroutine LastPositionCoroutine;
    private IEnumerator ChangePositionCoroutine(float start, float end, float duration)
    {
        float timer = 0.0f;
        while(timer <= 1.0f)
        {
            var lerp = Mathf.InverseLerp(0, duration, timer);
            var x = EasingFunction.EaseInOutQuad(start, end, lerp);
            MoveTransform.position = new Vector3(x, MoveTransform.position.y, MoveTransform.position.z);
            
            yield return null;
            timer += Time.deltaTime;
        }
    }

    // Some weird offset thing going on? Thanks unity inspector very good
    private bool Set;
    public static float DefaultX => DefaultXRaw * Screen.width;
    private static float DefaultXRaw;

    public static float HoverX => HoverXRaw * Screen.width;
    private static float HoverXRaw;

    public static float SelectedX => SelectedXRaw * Screen.width;
    private static float SelectedXRaw;


    public void OnPointerEnter(PointerEventData eventData)
    {
        if(this == MainMenu.Main.Selected) return;
        this.ChangeColor(HoverColor, 0.25f);
        this.ChangePosition(HoverX, 0.25f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(this == MainMenu.Main.Selected) return;
        this.ChangeColor(DefaultColor, 0.25f);
        this.ChangePosition(DefaultX, 0.25f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        MainMenu.Main.OnMenuButtonClick(this);
    }

    void Start()
    {
        ColorSprite.color = DefaultColor;

        if(!Set)
        {
            Set = true;
            DefaultXRaw = (MoveTransform.position.x - 0) / Screen.width;
            HoverXRaw = (MoveTransform.position.x - 40) / Screen.width;
            SelectedXRaw = (MoveTransform.position.x - 130) / Screen.width;
        }

        MoveTransform.position = new Vector3(DefaultX, MoveTransform.position.y, MoveTransform.position.z);
    }

    void Update()
    {
        
    }
}
