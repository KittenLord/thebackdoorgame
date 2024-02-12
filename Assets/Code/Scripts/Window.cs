using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowSettings
{
    public float DefaultAspectRatio { get; set; }
    public float DefaultWidthRatio { get; set; }
    public bool LockAspectRatio { get; set; }
    public bool LockScale { get; set; }

    public Color? BackgroundColor { get; set; }
    public Color? TitleBarColor { get; set; }
    public Color? TitleTextColor { get; set; }
}

public class Window : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private WindowSettings Settings;
    private WindowApplication Application;

    [SerializeField] private RectTransform ThisWindow;
    [SerializeField] private TMP_Text Title;
    [SerializeField] private RectTransform WorkingArea;

    private bool CanLoadApplication = true;
    public Application LoadApplication(string application)
    {
        CanLoadApplication = false;
        Debug.Log("abboa");
        var app = WindowApplication.Load<WindowApplication>(application);
        this.Application = Instantiate(app, WorkingArea);
        this.Application.Window = this;
        return this.Application;
    }

    void Start()
    {
        CanLoadApplication = false;
        ThisWindow = this.GetComponent<RectTransform>();
        UpdateSettings();
    }

    public void UpdateSettings()
    {
        this.Settings = Application.GetSettings();
        if(Settings.BackgroundColor is not null) WorkingArea.GetComponent<Image>().color = Settings.BackgroundColor.Value;
    }

    public void SetTitle(string text)
    {
        Title.text = text;
    }

    public void OnCloseButton()
    {
        Application.OnClosed();
    }

    public void Close() 
    {
        Destroy(this.gameObject);
    }

    private bool Drag = false;
    public void OnDrag(PointerEventData eventData) { if(Drag) ThisWindow.anchoredPosition += eventData.delta; }
    public void OnBeginDrag(PointerEventData eventData) { Drag = !WorkingArea.GetWorldRect(Vector2.one).Contains(eventData.position); }
    public void OnEndDrag(PointerEventData eventData) { Drag = false; }
}
