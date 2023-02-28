using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    [SerializeField]
    Image characterImage;
    [SerializeField]
    TextMeshProUGUI characterNameTextUGUI;
    [SerializeField]
    GameObject dialogueTextPanel;
    [SerializeField]
    TextMeshProUGUI dialogueTextUGUI;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendMessage(string text, Sprite characterSprite, string characterName)
    {
        characterImage.sprite = characterSprite;

    }
}
