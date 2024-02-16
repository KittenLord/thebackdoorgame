using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public static class KEY
{
    public const string SaveExists = "new";
    public const string SavedUsername = "username";
    public const string SavedPassword = "password";

    public static readonly string[] DeleteOnNew = new string[] { SavedUsername, SavedPassword };
}

public static class SavedData
{
    public static void Reset() => PlayerPrefs.DeleteAll();
    public static bool IsSaved(string key) => PlayerPrefs.HasKey(key);

    public static string GetString(string key) => PlayerPrefs.GetString(key);
    public static void SetString(string key, string value) => PlayerPrefs.SetString(key, value);

    public static void SetFlag(string flag, bool value = true) => PlayerPrefs.SetInt(flag, value ? 1 : 0);
    public static bool GetFlag(string flag) => PlayerPrefs.HasKey(flag) && PlayerPrefs.GetInt(flag) != 0;

    public static void SetJson(string id, object o) => PlayerPrefs.SetString(id, JsonConvert.SerializeObject(o));
    public static T GetJson<T>(string id, System.Func<string, string> edit) { try { return JsonConvert.DeserializeObject<T>(edit(PlayerPrefs.GetString(id))); } catch { return default; }}

    public static void RemoveKeys(params string[] keys) => keys.ToList().ForEach(key => PlayerPrefs.DeleteKey(key));
}
