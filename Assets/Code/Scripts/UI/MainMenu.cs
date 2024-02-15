using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class MainMenu : MonoBehaviour
{
    public static MainMenu Main { get; private set; }

    public List<MainMenuPanel> Panels { get; private set; } = new();
    [SerializeField] private Transform PanelHolder;

    void Awake()
    {
        Main = this;
    }
    
    void Start()
    {
        PanelHolderShownX = PanelHolder.position.x / Screen.width;
        PanelHolderHiddenX = (PanelHolder.position.x + 130 * (Screen.width / 1920)) / Screen.width;
        PanelHolder.transform.position = new Vector3(PanelHolderHiddenX * Screen.width, PanelHolder.position.y, PanelHolder.position.z);
        PanelHolder.SetAsLastSibling();
    }

    public void NewGame()
    {
        SavedData.RemoveKeys(KEY.DeleteOnNew);
        StartGame(0);
    }

    public void QuitGame()
    {
        UnityEngine.Application.Quit();
        Debug.Log("QUIT");
    }

    public static void StartGameStatic(int level)
    {
        Game.SelectedLevel = level;
        Debug.Log(level);
        SceneManager.LoadScene("GameScene");
    }

    public void StartGame(int level) => StartGameStatic(level);
    public void StartGame(Transform button) { StartGame(button.GetSiblingIndex()); }

    public async void SelectMenu(string menuId)
    {
        var panel = Panels.Find(p => p.Id == menuId);
        var disablePanels = Panels.Where(p => p != panel).ToList();

        disablePanels.ForEach(p => p.Hide(0.25f));

        ShowPanelHolder();
        await System.Threading.Tasks.Task.Delay(150);

        panel?.Show(0.25f);
    }

    public async void DeselectAllMenus()
    {
        Panels.ForEach(p => p.Hide(0.25f));
        await System.Threading.Tasks.Task.Delay(200);
        HidePanelHolder();
    }

    private float PanelHolderShownX;
    private float PanelHolderHiddenX;

    public void ShowPanelHolder()
    {
        if(LastPositionCoroutine is not null) StopCoroutine(LastPositionCoroutine);
        LastPositionCoroutine = StartCoroutine(ChangePositionCoroutine(PanelHolder.position.x, PanelHolderShownX * Screen.width));
    }
    public void HidePanelHolder()
    {
        if(LastPositionCoroutine is not null) StopCoroutine(LastPositionCoroutine);
        LastPositionCoroutine = StartCoroutine(ChangePositionCoroutine(PanelHolder.position.x, PanelHolderHiddenX * Screen.width));
    }

    private Coroutine LastPositionCoroutine;
    private IEnumerator ChangePositionCoroutine(float start, float end)
    {
        float timer = 0.0f;

        var duration = Mathf.Abs(start - end) / (130.0f * 4);
        duration = Mathf.Max(duration, 0.1f);

        while(timer <= 1.0f)
        {
            var lerp = Mathf.InverseLerp(0, duration, timer);
            var x = EasingFunction.EaseInOutQuad(start, end, lerp);
            PanelHolder.position = new Vector3(x, PanelHolder.position.y, PanelHolder.position.z);
            
            yield return null;
            timer += Time.deltaTime;
        }
    }

    public MainMenuButton Selected { get; private set; }
    public void DeselectMenuButtons()
    {
        Selected?.ChangeColor(MainMenuButton.DefaultColor, 0.25f);
        Selected?.ChangePosition(MainMenuButton.DefaultX, 0.25f);
        Selected = null;
    }
    public void OnMenuButtonClick(MainMenuButton button)
    {
        if(Selected == button) return;
        Selected?.ChangeColor(MainMenuButton.DefaultColor, 0.25f);
        Selected?.ChangePosition(MainMenuButton.DefaultX, 0.25f);
        Selected = button;
        Selected.ChangeColor(MainMenuButton.SelectedColor, 0.25f);
        Selected.ChangePosition(MainMenuButton.SelectedX, 0.25f);
    }




    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F12)) SavedData.Reset();
    }
}
