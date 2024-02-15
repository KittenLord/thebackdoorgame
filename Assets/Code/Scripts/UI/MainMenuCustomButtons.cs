using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public partial class MainMenu : MonoBehaviour {

    public void Button_SignIn_New() 
    {
        var hasSave = SavedData.GetFlag(KEY.SaveExists);
        if(!hasSave)
        {
            this.NewGame();
            return;
        }
        
        this.SelectMenu("signin_newconfirm");
    }

}