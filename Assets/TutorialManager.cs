using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{

    public Image bgUp, bgDown;
    public GameObject[] panelsTop, panelsBot;
    public float[] panelsDelimiter;
    public float delimiter = 0.75f, currentDelimiter = 0.75f;
    public float delimiterSpeed = 1.0f;
    private int currentPanel = -1;
    private bool changing = true;

    void Start()
    {
    }

    // Use this for initialization
    void Awake()
    {
        ClearPanels();
        delimiter = panelsDelimiter[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (! changing && Input.GetMouseButtonDown(0))
        {
            if (currentPanel == panelsBot.Length - 1) Application.LoadLevel("Game");
            else
            {
                ClearPanels();
                changing = true;
                delimiter = panelsDelimiter[currentPanel + 1];
            }
        }
        if (changing)
        {
            CorrectDelimiter();
            if (Mathf.Abs(delimiter - currentDelimiter) < 0.01)
            {
                NextPanel();
                changing = false;
            }
        }
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
        panelsTop[currentPanel].SetActive(true);
        panelsBot[currentPanel].SetActive(true);
    }

    void ClearPanels()
    {
        foreach (GameObject panel in panelsTop)
        {
            panel.SetActive(false);
        }

        foreach (GameObject panel in panelsBot)
        {
            panel.SetActive(false);
        }
    }
}

