using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
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

    private int index;
    private string actualText = "";

    public float speed = 10.0f;
    private float baseSpeed = 10.0f;
    public bool isPlaying = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(isPlaying == false)
            {
                //actualText = string.Empty;
                StartCoroutine(SendMessage("This is a test message, wahoo! This is <b>bold text</b>! This is <i>italic text</i>! " +
                    "<speed=30>This is fast written text!</speed> This is now normally written text again!", "Greg", TransitionTypes.FADE));
            }
            else
            {
                isPlaying = false;
            }
        }
    }

    public IEnumerator SendMessage(string text, string characterName, TransitionTypes charTransition = TransitionTypes.NONE, float charTransParam = 1.0f)
    {
        yield return StartCoroutine(SendMessage(text, null, characterName, charTransition, charTransParam));
    }
    public IEnumerator SendMessage(string text, Sprite characterSprite, string characterName,
        TransitionTypes characterTransition = TransitionTypes.NONE, float charTransParam = 1.0f)
    {
        isPlaying = true;
        if(characterSprite != null)
            characterImage.sprite = characterSprite;
        characterNameTextUGUI.text = characterName;

        //The character transition typically happens before anything else appears
        yield return StartCoroutine(DoImageTransition(characterTransition, charTransParam));

        characterNameTextUGUI.gameObject.SetActive(true);
        dialogueTextPanel.SetActive(true);
        dialogueTextUGUI.text = string.Empty;
        dialogueTextUGUI.gameObject.SetActive(true);

        string invisTag = @"<alpha=#00>";

        dialogueTextUGUI.text = text;
        for(int i = 0; i < text.Length; i++)
        {
            ParseTag(ref i, ref text);
            string splicedText = text.Substring(0, i + 1) + invisTag + text.Substring(i + 1);
            dialogueTextUGUI.text = splicedText;
            if(isPlaying == true)
            {
                yield return new WaitForSeconds(1.0f / speed);
                yield return StartCoroutine(WaitForPunctuation(text[i]));
            }
        }
        isPlaying = false;
        yield return null;
    }

    #region Text Parsing Functions
    IEnumerator WaitForPunctuation(char letter)
    {
        switch(letter)
        {
            case '!':
            case '?':
            case '.':
                {
                    yield return new WaitForSeconds(7.5f / speed);
                    break;
                }
            case ',':
                {
                    yield return new WaitForSeconds(3.5f / speed);
                    break;
                }
            default:
                {
                    yield break;
                }
        }
    }

    void ParseTag(ref int index, ref string text)
    {
        if (text[index] == '<')
        {
            string fullTag = text.Substring(index);
            int closingIndex = fullTag.IndexOf('>');
            index += closingIndex + 1;
            fullTag = fullTag.Substring(0, closingIndex + 1);

            //at this point dupl has the full tag, complete with <>
            string bareTag = fullTag.Substring(1, fullTag.Length - 2);

            //
            if(bareTag.Contains("speed"))
            {
                index -= closingIndex + 1;
                text = text.Remove(index, closingIndex + 1);
                if (bareTag.Contains("/"))
                {
                    speed = baseSpeed;
                    return;
                }
                int equalIndex = bareTag.IndexOf("=");
                //in case the tag is just 'speed', return
                if(equalIndex == -1)
                {
                    return;
                }
                //in case the tag is just 'speed=' also return
                if(equalIndex == bareTag.Length - 1)
                {
                    return;
                }
                float val;
                try
                {
                    val = float.Parse(bareTag.Substring(equalIndex + 1));
                    speed = val;
                    return;

                }
                catch(Exception e)
                {
                    return;
                }

            }
            actualText += fullTag;

        }
    }
    #endregion

    #region Transition Functions

    IEnumerator DoImageTransition(TransitionTypes transType, float param)
    {
        characterImage.gameObject.SetActive(true);
        switch (transType)
        {
            case TransitionTypes.FADE:
                yield return StartCoroutine(Fade(param));
                break;
        }
        yield return null;
    }

    IEnumerator Fade(float param)
    {
        Color c = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        for(float i = 0.0f; i <= param; i+= Time.deltaTime)
        {
            c.a = Mathf.Lerp(0.0f, 1.0f, i / param);
            characterImage.color = c;
            yield return null;
        }
    }
    #endregion
}
