using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PermissionsApplication : WindowApplication
{
    public override string Name => "perms";
    [SerializeField] private Transform CategoryPrefab;
    [SerializeField] private Transform CategoryParent;

    [SerializeField] private Transform EntryPrefab;
    [SerializeField] private GameObject EntryInput;

    [SerializeField] private Button ConfirmButton;

    public string TargetPath { get; set; }

    [SerializeField] private TMP_Text TargetPathDisplay;

    private Computer.FileNavigator navigator;

    async void Start()
    {
        // This is some absolutely vile code, but it's a gamejam

        try{
        ConfirmButton.onClick.AddListener(() => {
            this.Window.OnCloseButton();
        });
        
        navigator = Handle.GetNavigator(ProcessId);
        var file = navigator.GetFile(TargetPath);
        TargetPathDisplay.text = TargetPath;
        if(file is null) { Debug.Log(TargetPath); return; }

        var result = navigator.GetFilePermissions(TargetPath, out var permissions);
        if(result is not null) { Debug.Log(result); return; }

        if(!permissions.Fit(Handle.GetProcess(ProcessId).Access, FilePermission.Manage, FilePermission.ManageInner)) { Debug.Log("dawdadasd"); return; }

        Debug.Log(TargetPath + " " + permissions[FilePermission.Manage].FirstOrDefault());
        var editPermissions = file.Permissions.Copy();
        Debug.Log(TargetPath + " " + editPermissions[FilePermission.Manage].FirstOrDefault());

        ConfirmButton.onClick.AddListener(() => {
            file.Permissions = editPermissions;
        });

        var categories = Enum.GetValues(typeof(FilePermission)).Cast<FilePermission>().ToList();
        Debug.Log(categories.Count);
        foreach(var categoryPermission in categories)
        {
            var category = Instantiate(CategoryPrefab, CategoryParent);
            category.GetChild(0).GetComponent<TMP_Text>().text = categoryPermission.ToString().ToUpper();

            var entries = category.GetChild(1).GetChild(0);
            var permissionEntries = editPermissions[categoryPermission];
            var plus = entries.GetChild(0);

            plus.GetComponent<Button>().onClick.AddListener(() => {
                EntryInput.SetActive(true);
                var input = EntryInput.transform.GetChild(1).GetComponent<TMP_InputField>();
                input.onEndEdit.RemoveAllListeners();

                EventSystem.current.SetSelectedGameObject(input.gameObject, null);
                input.OnPointerClick(new PointerEventData(EventSystem.current));

                input.onEndEdit.AddListener(async (s) => {
                    EntryInput.SetActive(false);
                    if(input.wasCanceled) return;
                    System.Text.RegularExpressions.Regex r = new(@"^(\d )?\S+$");
                    if(!r.IsMatch(s)) { Debug.Log($"regex fail: \"{s}\""); return; }
                    if(!s.Contains(" "))
                    {
                        if(!Handle.UserExists(s) && !int.TryParse(s, out var _)) return;
                        editPermissions[categoryPermission].Add(s);
                    }
                    else
                    {
                        var split = s.Split(" ");
                        if(!Handle.UserExists(split[1])) return;
                        if(!int.TryParse(split[0], out var i)) return;
                        editPermissions[categoryPermission].Add(s);
                    }

                    var entry = Instantiate(EntryPrefab, entries);
                    entry.GetChild(0).GetComponent<TMP_Text>().text = s;
                    input.text = "";
                    plus.SetAsLastSibling();

                    entry.GetComponent<Button>().onClick.AddListener(() => {
                        editPermissions[categoryPermission].Remove(s);
                        Destroy(entry.gameObject);
                    });

                    var l = entries.GetComponent<HorizontalLayoutGroup>();
                    l.childScaleWidth = !l.childScaleWidth;
                    await Task.Delay(5); // thanks fucking unity very cool
                    l.childScaleWidth = !l.childScaleWidth;
                });
            });

            foreach(var pe in permissionEntries)
            {
                var entry = Instantiate(EntryPrefab, entries);
                plus.SetAsLastSibling();
                entry.GetChild(0).GetComponent<TMP_Text>().text = pe;

                entry.GetComponent<Button>().onClick.AddListener(() => {
                    editPermissions[categoryPermission].Remove(pe);
                    Destroy(entry.gameObject);
                });
            }
            var l = entries.GetComponent<HorizontalLayoutGroup>();
            l.childScaleWidth = !l.childScaleWidth;
            await Task.Delay(5); // thanks fucking unity very cool
            l.childScaleWidth = !l.childScaleWidth;

        }
        }catch(Exception e){Debug.Log(e);}
    }

    public override WindowSettings GetSettings()
    {
        return new();
    }
}