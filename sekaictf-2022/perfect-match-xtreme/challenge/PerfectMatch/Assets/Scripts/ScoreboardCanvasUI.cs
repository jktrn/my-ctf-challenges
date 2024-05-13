using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardCanvasUI : MonoBehaviour
{
    public GameObject timerBoard;
    public GameObject fruitImage;
    
    private GameObject timerText;
    private Grid grid;
    private GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        timerText = FindObjectOfType<UI>().timerText;
        grid = FindObjectOfType<Grid>();
        gameManager = FindObjectOfType<GameManager>();
        
        timerBoard.GetComponentInChildren<TextMeshProUGUI>().text =
            timerText.GetComponentInChildren<TextMeshProUGUI>().text;
    }

    // Update is called once per frame
    void Update()
    {
        timerBoard.SetActive(grid.canStart && !grid.revealingFruit && !gameManager.isGameOver);
        SetChosenFruit(grid.revealingFruit && !gameManager.isGameOver);
        
        timerBoard.GetComponentInChildren<TextMeshProUGUI>().text =
            timerText.GetComponentInChildren<TextMeshProUGUI>().text;
    }

    void SetChosenFruit(bool revealing)
    {
        if (revealing)
        {
            fruitImage.GetComponentInChildren<Image>().sprite = gameManager.chosenFruit;
        }
        fruitImage.SetActive(revealing);
    }
}
