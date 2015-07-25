using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour {

    public Image bgUp, bgDown;
    public GameObject[] panelsTop, panelsBot;
    public float[] panelsDelimiter; 
    public float delimiter = 0.75f, currentDelimiter = 0.75f;
    public float delimiterSpeed = 1.0f;
    private int currentPanel = 0;

    void Start()
    {
    }

	// Use this for initialization
	void Awake () {
        foreach (GameObject panel in panelsTop)
        {
            panel.SetActive(false);
        }

        foreach (GameObject panel in panelsBot)
        {
            panel.SetActive(false);
        }
        panelsTop[0].SetActive(true);
        panelsBot[0].SetActive(true);
        delimiter = panelsDelimiter[0];
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            if (currentPanel == panelsBot.Length-1) Application.LoadLevel("Game");
            else NextPanel();
        }
        CorrectDelimiter();

	}

    void CorrectDelimiter()
    {
        currentDelimiter = Mathf.Lerp(currentDelimiter, delimiter, Time.deltaTime * delimiterSpeed);
        bgUp.GetComponent<LayoutElement>().flexibleHeight = currentDelimiter;
        bgDown.GetComponent<LayoutElement>().flexibleHeight = 1.0f - currentDelimiter;
    }

    void NextPanel()
    {
        ++currentPanel;
        UpdatePanel();
    }

    void UpdatePanel()
    {

        panelsTop[currentPanel-1].SetActive(false);
        panelsBot[currentPanel-1].SetActive(false);

        panelsTop[currentPanel].SetActive(true);
        panelsBot[currentPanel].SetActive(true);
        delimiter = panelsDelimiter[currentPanel];
    }
}
