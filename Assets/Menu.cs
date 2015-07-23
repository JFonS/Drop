using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour 
{
	void Start () {}
	void Update () {}

    public void GoToGame()
    {
        Debug.Log("aasd");
        Application.LoadLevel("Game");
    }

    public void GoToTutorial()
    {
        Debug.Log("aasd");
        Application.LoadLevel("Tutorial");
    }
}
