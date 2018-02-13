using UnityEngine;

public class SceneChange : MonoBehaviour {

    public void GoTo_MainMenu() {
        Debug.Log( "going to main menu" );
        Application.LoadLevel( "Menu_Main" );
    }

    public void GoTo_HelpMenu() {
        Debug.Log( "going to help menu" );
        Application.LoadLevel( "Menu_Help" );
    }

    public void GoTo_StatsMenu() {
        Debug.Log( "going to stat menu" );
        Application.LoadLevel( "Menu_Stats" );
    }

    public void GoTo_CreditsMenu() {
        Debug.Log( "going to credits menu" );
        Application.LoadLevel( "Menu_Credits" );
    }

    public void GoTo_GameOver() {
        Debug.Log( "going to game over menu" );
        Application.LoadLevel( "Menu_GameOver" );
    }

    public void GoTo_PlayMenu() {
        Debug.Log( "going to play menu" );
        Application.LoadLevel( "Menu_StartGame" );
    }

    public void GoTo_PlayGame() {
        Debug.Log( "going to play game" );
        Application.LoadLevel( "Scene_Play" );
    }

    public void GoTo_ExitGame() {
        Debug.Log( "going to exit game" );
        Application.Quit();
    }

}