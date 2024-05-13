using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public GameObject timerText;
    public GameObject gameStateUI;
    public TextMeshProUGUI text11;
    public TextMeshProUGUI text12;
    public TextMeshProUGUI text13;
    public TextMeshProUGUI text14;
    public TextMeshProUGUI text15;
    public TextMeshProUGUI text16;
    public TextMeshProUGUI text21;
    public TextMeshProUGUI text22;
    public TextMeshProUGUI text23;
    public TextMeshProUGUI text31;
    public TextMeshProUGUI text32;
    public TextMeshProUGUI text33;
    public TextMeshProUGUI gameStateText;
    public TextMeshProUGUI countdownText;
    private Grid grid;
    
    private float threeSecondTimer = 3.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
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
            text11.gameObject.SetActive(true);
            text12.gameObject.SetActive(true);
            text13.gameObject.SetActive(true);
            text14.gameObject.SetActive(true);
            text15.gameObject.SetActive(true);
            text16.gameObject.SetActive(true);
            text21.gameObject.SetActive(true);
            text22.gameObject.SetActive(true);
            text23.gameObject.SetActive(true);
            text31.gameObject.SetActive(true);
            text32.gameObject.SetActive(true);
            text33.gameObject.SetActive(true);
        }
        else
        {
            SetGameStateText("Eliminated! No flag :(");
        }

        Time.timeScale = 0.0f;
    }

}
