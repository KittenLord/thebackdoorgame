using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class SoorchPage
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string CustomType { get; set; }
    public string Content { get; set; }
}

public class SoorchApplication : WindowApplication
{
    private List<SoorchPage> Pages;
    void Start() { SearchTab.gameObject.SetActive(true); Pages = JsonConvert.DeserializeObject<List<SoorchPage>>(Resources.Load<TextAsset>("articles").text); }
    public override string Name => "soorch";
    public override WindowSettings GetSettings()
    {
        return new();
    }



    [SerializeField] private Transform SearchTab;
        [SerializeField] private TMP_InputField SearchInput;

    [SerializeField] private Transform ResultTab;
        [SerializeField] private Transform ResultList;
        [SerializeField] private Transform ResultPrefab;

    [SerializeField] private Transform ArticleTab;
    [SerializeField] private Transform CustomTab;
        [SerializeField] private TMP_InputField AmethystUsername;
        [SerializeField] private TMP_InputField AmethystPassword;
        [SerializeField] private TMP_Text AmethystError;

    [SerializeField] private Transform Page404;


    public void HidePage()
    {
        ArticleTab.gameObject.SetActive(false);
        CustomTab.gameObject.SetActive(false);
        Page404.gameObject.SetActive(false);
    }

    public void GoHome()
    {
        ArticleTab.gameObject.SetActive(false);
        CustomTab.gameObject.SetActive(false);
        ResultTab.gameObject.SetActive(false);
        Page404.gameObject.SetActive(false);
        SearchTab.gameObject.SetActive(true);
    }

    void Update()
    {
        if(Time.frameCount % 20 != 0) return;
        var amethyst = CustomTab.Find("AmethystClub");
        if(amethyst is null) return;

        var computer = Game.Current.Stage.Computers.Find(c => c.Users.ContainsKey("amethyst") && c != Game.Current.computer);
        if(computer is not null) return;

        if(amethyst.gameObject.activeSelf) Page404.gameObject.SetActive(true);
        Destroy(amethyst.gameObject);
    }

    public void OnAmethystLogin()
    {
        AmethystError.text = "";
        System.Text.RegularExpressions.Regex r = new(@""";ERROR\(SELECT\(env\.(?<value>.*)\)\);--");
        var a = AmethystUsername.text.Replace(" ", "");
        var b = AmethystPassword.text.Replace(" ", "");

        var computer = Game.Current.Stage.Computers.Find(c => c.Users.ContainsKey("amethyst_giraffeql") && c != Game.Current.computer);
        if(computer is null) { Page404.gameObject.SetActive(true); return; }

        if(!r.IsMatch(a) && !r.IsMatch(b))
        {
            AmethystError.text = "Account with this username isn't registered. Did you get an invite?";
            return;
        }

        var result = r.IsMatch(a) ? r.Match(a) : r.Match(b);
        var value = result.Groups["value"].Value;
        if(value != "username" && value != "password" && value != "ip" && value != "trophy")
        {
            AmethystError.text = $"INTERNAL GIRAFFEQL ERROR: 'env' doesn't have property '{value}'";
            return;
        }

        if(value == "username")
        {
            AmethystError.text = $"GIRAFFEQL QUERY ERROR: amethyst_giraffeql";
            return;
        }

        if(value == "password")
        {
            AmethystError.text = $"GIRAFFEQL QUERY ERROR: {Game.Current.Operator.State["GIRAFFEQLPASSWORD_REPLACE"]}";
            return;
        }

        if(value == "ip")
        {
            AmethystError.text = $"GIRAFFEQL QUERY ERROR: {Game.Current.Operator.State["GIRAFFEQLIP_REPLACE"]}";
            return;
        }

        if(value == "trophy")
        {
            AmethystError.text = $"GIRAFFEQL QUERY ERROR: {Game.Current.Operator.State[LevelOperator.Trophy1]}";
            return;
        }
    }

    public void SearchButton()
    {
        var query = SearchInput.text;
        if(query == "") return;
        if(query.StartsWith("https://")) 
        {
            var page = Pages.Find(p => p.Url == query);
            if(page is null) return;
                if(page.CustomType == "")
                {
                    //ResultTab.gameObject.SetActive(false);
                    ArticleTab.gameObject.SetActive(true);

                    ArticleTab.GetChild(0).GetComponent<TMP_Text>().text = page.Title;
                    ArticleTab.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = page.Content;
                    return;
                }

                CustomTab.gameObject.SetActive(true);
                foreach(Transform o in CustomTab) o.gameObject.SetActive(false);
                CustomTab.Find(page.CustomType).gameObject.SetActive(true);
            return;
        }

        var words = query.Split(" ").Where(w => w != "" && char.IsLetterOrDigit(w[0])).Select(w => w.ToLower());
        int EvaluatePage(SoorchPage page)
        {
            var titleWords = page.Title.Split(" ").Select(w => w.ToLower()).ToList();
            var titleWordsCopy = page.Title.Split(" ").Select(w => w.ToLower()).ToList();
            titleWords.AddRange(titleWordsCopy.Select(w => new string(w.Take(3).ToArray())));
            return titleWords.Count(tw => words.Any(w => w.StartsWith(tw)));
        }
        var results = Pages.OrderByDescending(p => EvaluatePage(p)).Where(p => EvaluatePage(p) > 0).Take(10);

        SearchTab.gameObject.SetActive(false);
        ResultTab.gameObject.SetActive(true);

        foreach(Transform child in ResultList) Destroy(child.gameObject);

        foreach(var page in results)
        {
            var p = page;
            var result = Instantiate(ResultPrefab, ResultList);

            result.GetChild(0).GetComponent<TMP_Text>().text = page.Title;
            result.GetChild(1).GetComponent<TMP_Text>().text = page.Url;

            result.GetComponent<Button>().onClick.AddListener(() => {
                if(p.CustomType == "")
                {
                    //ResultTab.gameObject.SetActive(false);
                    ArticleTab.gameObject.SetActive(true);

                    ArticleTab.GetChild(0).GetComponent<TMP_Text>().text = p.Title;
                    ArticleTab.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = p.Content;
                    return;
                }

                CustomTab.gameObject.SetActive(true);
                foreach(Transform o in CustomTab) o.gameObject.SetActive(false);
                var tab = CustomTab.Find(page.CustomType);
                if(tab is not null) tab.gameObject.SetActive(true);
                else Page404.gameObject.SetActive(true);
            });
        }
    }
}