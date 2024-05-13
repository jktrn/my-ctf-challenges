using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public GameObject timerText;
    public GameObject gameStateUI;
    public GameObject parts;
    public TextMeshProUGUI gameStateText;
    public TextMeshProUGUI countdownText;
    private Grid grid;
    
    private float threeSecondTimer = 3.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        grid = FindObjectOfType<Grid>();
    }

    // Update is called once per frame
    void Update()
    {
        timerText.SetActive(grid.revealingFruit);
        gameObject.SetActive(true);
        
        countdownText.gameObject.SetActive(true);
        countdownText.text = Mathf.Round(threeSecondTimer).ToString();
        if (threeSecondTimer < 0)
        {
            grid.canStart = true;
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            // Countdown.
            threeSecondTimer -= Time.deltaTime;
        }
    }

    public void SetText(string text)
    {
        timerText.GetComponent<TextMeshProUGUI>().text = text;
    }
    
    public void SetGameStateText(string text)
    {
        gameStateText.text = text;
    }

    public void SetGameState(bool isWon)
    {
        gameStateUI.SetActive(true);
        gameStateText.gameObject.SetActive(true);
        if (isWon)
        {
            SetGameStateText("Qualified! Have your flag :)");
            parts.SetActive(true);
        }
        else
        {
            SetGameStateText("Eliminated! No flag :(");
        }

        Time.timeScale = 0.0f;
    }

}
