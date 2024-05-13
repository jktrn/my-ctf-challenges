using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformTile : MonoBehaviour
{
    private Image image;
    private Canvas canvas;
    private void Awake()
    {
        canvas = transform.GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        image = canvas.GetComponentInChildren<Image>();
    }

    public void SetImage(Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void ToggleImage()
    {
        canvas.gameObject.SetActive(!canvas.gameObject.activeSelf);
    }

    public void SetCanvasActive(bool active)
    {
        canvas.gameObject.SetActive(active);
    }
}

/*
 First Round:  2 Fruits, 8 each
 Second Round: 4 Fruits, 4 each
 Third Round:  6 Fruits, 2-3 each
 
 Each consecutive round being inclusive of fruit from previous rounds
 
 And once already chosen no fruit can be chosen again.
*/