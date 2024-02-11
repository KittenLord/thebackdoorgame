using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowSettings
{
    public float DefaultAspectRatio { get; set; }
    public float DefaultWidthRatio { get; set; }
    public bool LockAspectRatio { get; set; }
    public bool LockScale { get; set; }
}

public class Window : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private WindowSettings Settings;
    private Application Application;

    [SerializeField] private RectTransform ThisWindow;
    [SerializeField] private RectTransform WorkingArea;

    private bool CanLoadApplication = true;
    public void LoadApplication(string application, Computer computer)
    {
        CanLoadApplication = false;
        var path = $"Applications/{application}";
        var app = Resources.Load<Application>(path);
        this.Application = Instantiate(app, WorkingArea);
        this.Application.SetContext(computer, this);
    }

    void Start()
    {
        CanLoadApplication = false;
        ThisWindow = this.GetComponent<RectTransform>();
        this.Settings = Application.GetSettings();
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
