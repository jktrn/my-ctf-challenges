using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightCheck : MonoBehaviour
{
    private GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.position.y < -20)
        {
            gameManager.hasFailed = true;
            Time.timeScale = 0.0f;
        }
    }
}
