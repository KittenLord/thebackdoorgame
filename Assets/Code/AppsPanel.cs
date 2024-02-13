using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppsPanel : MonoBehaviour
{
    [SerializeField] private Transform TabPrefab;
    [SerializeField] private Transform TabParent;

    public class Tab
    {
        public Transform Transform;
        public Window Window;

        public Tab(Transform t, Window w)
        {
            Transform = t; Window = w;
        }
    }

    private List<Tab> Tabs = new();

    void Update()
    {
        var windows = new List<Window>();
        foreach(Transform obj in Game.Current.Canvas.transform) windows.Add(obj.GetComponent<Window>());
        windows.RemoveAll(w => w is null);

        var toDelete = Tabs.Where(tab => tab.Window == null);
        var toCreate = windows.Where(window => !Tabs.Any(tab => tab.Window == window));

        Tabs.RemoveAll(l => toDelete.Contains(l));
        foreach(var delete in toDelete) { Destroy(delete.Transform.gameObject); }
        foreach(var create in toCreate)
        {
            var tab = new Tab(Instantiate(TabPrefab, TabParent), create);
            tab.Transform.GetChild(1).GetComponent<TMP_Text>().text = create.Title.text;
            tab.Transform.GetComponent<Button>().onClick.AddListener(() => {
                if(tab.Window.transform.position.y < -10000) tab.Window.transform.position += new Vector3(0, 1, 0) * 20000;
                else tab.Window.transform.position -= new Vector3(0, 1, 0) * 20000;
            });

            Tabs.Add(tab);
        }

        transform.SetAsLastSibling();
    }
}
