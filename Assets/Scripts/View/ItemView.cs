using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemView : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private Text itemName;
    [SerializeField] private Button showButton;

    public Button ShowButton => showButton;

    public Text ItemName => itemName;

    public void SetImage(Sprite image)
    {
        itemImage.sprite = image;
    }

    public void SetName(string name)
    {
        itemName.text = name;
    }

    public void DisplayButton(bool isCompleteForShow)
    {
        showButton.gameObject.SetActive(isCompleteForShow);
    }
}
