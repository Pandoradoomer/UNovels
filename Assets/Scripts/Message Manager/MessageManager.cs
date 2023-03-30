using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI characterName;
    [SerializeField]
    private TextMeshProUGUI dialogueText;
    [SerializeField]
    private Image characterImage;
    [SerializeField]
    private GameObject textBox;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Image backgroundImage;

    bool isMessageRunning = false;
    private bool isSpeeding = false;
    bool waitingForInput = false;

    public float speed = 20.0f;
    public float baseSpeed = 20.0f;
    public string currentText = "";
    public float tagWait = -1;

    float boxColorAlpha = 0.0f;

    public static MessageManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        boxColorAlpha = textBox.GetComponent<Image>().color.a;
        if(mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isMessageRunning)
            {
                isMessageRunning = false;
            }
            else
            {
                if (waitingForInput)
                    waitingForInput = false;
            }
        }
    }

    public IEnumerator LoadScene(SceneEditor scene)
    {
        backgroundImage.sprite = scene.backgroundImage;
        switch(scene.entryTransition)
        {
            case TransitionTypes.NONE:
                backgroundImage.gameObject.SetActive(true);
                yield return null;
                break;
            case TransitionTypes.FADE:
                yield return StartCoroutine(FadeInBackground(scene.entryTransitionValue));
                break;
        }
        yield return null;
    }
    public IEnumerator FadeInBackground(float time)
    {
        Color c = new Color(1, 1, 1, 0);
        backgroundImage.color = new Color(1, 1, 1, 0);
        backgroundImage.gameObject.SetActive(true);
        for(float i = 0.0f; i < time; i += Time.deltaTime)
        {
            c.a = Mathf.Lerp(0, 1, i / time);
            backgroundImage.color = c;
            yield return null;
        }
    }
    public IEnumerator DisplayMessage(CommandData dialogue)
    {
        characterName.text = dialogue.Character.characterName;
        characterName.color = dialogue.Character.nameColor;
        dialogueText.color = dialogue.Character.dialogueColor;

        string invisTag = "<alpha=#00>";
        string text = dialogue.dialogueText;
        dialogueText.text = text;
        isMessageRunning = true;
        for (int i = 0; i < text.Length; i++)
        {
            while (ParseTag(ref i, ref text)) ;
            if (tagWait != -1)
                yield return new WaitForSeconds(tagWait);
            string splicedText = text.Substring(0, i + 1) + invisTag + text.Substring(i + 1);
            dialogueText.text = splicedText;
            if (isMessageRunning)
            {
                yield return StartCoroutine(WaitForPunctuation(text[i]));
            }

        }
        isMessageRunning = false;
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

    bool ParseTag(ref int index, ref string text)
    {
        if (text[index] == '<')
        {
            string fullTag = text.Substring(index);
            int closingIndex = fullTag.IndexOf('>');
            index += closingIndex + 1;
            fullTag = fullTag.Substring(0, closingIndex + 1);

            //at this point dupl has the full tag, complete with <>
            string bareTag = fullTag.Substring(1, fullTag.Length - 2);

            if (bareTag.Contains("speed"))
            {
                index -= closingIndex + 1;
                text = text.Remove(index, closingIndex + 1);
                if (bareTag.Contains("/"))
                {
                    speed = baseSpeed;
                    return true;
                }
                int equalIndex = bareTag.IndexOf("=");
                //in case the tag is just 'speed', return
                if (equalIndex == -1)
                {
                    return true;
                }
                //in case the tag is just 'speed=' also return
                if (equalIndex == bareTag.Length - 1)
                {
                    return true;
                }
                float val;
                try
                {
                    val = float.Parse(bareTag.Substring(equalIndex + 1));
                    speed = val;
                    return true;

                }
                catch (Exception e)
                {
                    return true;
                }

            }
            else if (bareTag.Contains("punch"))
            {
                index -= closingIndex + 1;
                text = text.Remove(index, closingIndex + 1);
                StartCoroutine(Punch());
                tagWait = 0.5f;
                return true;
            }
            //currentText += fullTag;
            return true;
        }
        return false;
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
    IEnumerator Punch()
    {
        Vector3 originalPosCam = mainCamera.transform.position;
        Vector3 originalPosImage = characterImage.transform.position;
        Vector3 originalPosTextbox = textBox.transform.position;
        float shakeAmount = 0.7f;
        for (float i = 0; i < 0.5f; i += Time.deltaTime)
        {
            Vector3 randPos = UnityEngine.Random.insideUnitSphere;
            mainCamera.transform.position = originalPosCam + randPos * shakeAmount;
            characterImage.transform.position = originalPosImage + randPos * shakeAmount;
            textBox.transform.position = originalPosTextbox + randPos * shakeAmount;
            yield return null;
        }
        mainCamera.transform.position = originalPosCam;
        characterImage.transform.position = originalPosImage;
        textBox.transform.position = originalPosTextbox;
        tagWait = -1;
    }

    IEnumerator FadeTextBoxAway(bool isInverted)
    {
        textBox.SetActive(true);
        Image textBoxImg = textBox.GetComponent<Image>();
        Color charColor = characterName.color;
        Color textColor = dialogueText.color;
        Color boxColor = textBoxImg.color;
        float dur = 0.75f;
        for (float i = 0; i <= dur; i += Time.deltaTime)
        {
            if (isInverted)
            {
                charColor.a = Mathf.Lerp(0, 1, (dur - i) / dur);
                textColor.a = Mathf.Lerp(0, 1, (dur - i) / dur);
                boxColor.a = Mathf.Lerp(0, boxColorAlpha, (dur - i) / dur);
            }
            else
            {

                charColor.a = Mathf.Lerp(0, 1, i / dur);
                textColor.a = Mathf.Lerp(0, 1, i / dur);
                boxColor.a = Mathf.Lerp(0, boxColorAlpha, i / dur);
            }

            characterName.color = charColor;
            dialogueText.color = textColor;
            textBoxImg.color = boxColor;
            yield return null;
        }
        if (isInverted)
            textBox.SetActive(false);
    }
    #endregion
}

/*
public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI characterName;
    [SerializeField]
    private TextMeshProUGUI dialogueText;
    [SerializeField]
    private Image backgroundBlur;
    [SerializeField]
    private Image characterImage;
    [SerializeField]
    private GameObject textBox;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Canvas canvas;

    bool isMessageRunning = false;
    private bool isSpeeding = false;
    bool waitingForInput = false;

    public float speed = 20.0f;
    public float baseSpeed = 20.0f;
    public string currentText = "";
    public float tagWait = -1;

    float boxColorAlpha = 0.0f;
    void Start()
    {
        boxColorAlpha = textBox.GetComponent<Image>().color.a;
    }

    private void OnDestroy()
    {

    }

    IEnumerator BeginDialogueSequence(DialogueSequenceData sequence)
    {
        isMessageRunning = false;
        currSequence = sequence;
        characterImage.sprite = sequence.dialogueSequence[0].characterData.characterImage;
        StartCoroutine(BlurBackground(false));
        yield return StartCoroutine(FadeAndSlideCharacter(false));
        yield return StartCoroutine(StartMessageSequence(sequence.dialogueSequence));
        StartCoroutine(BlurBackground(true));
        StartCoroutine(FadeTextBoxAway(true));
        yield return StartCoroutine(FadeAndSlideCharacter(true));
        yield return null;
    }
    IEnumerator StartMessageSequence(List<DialogueData> sequence)
    {
        textBox.SetActive(true);
        characterName.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        for (int i = 0; i < sequence.Count; i++)
        {
            yield return StartCoroutine(DisplayMessage(sequence[i]));
            waitingForInput = true;
            while (waitingForInput)
            {
                yield return null;
            }
            //On 4th message enable keypress
            //User must select at least one of the weapons to enter the dungeon
        }
    }

    IEnumerator FadeTextBoxAway(bool isInverted)
    {
        textBox.SetActive(true);
        Image textBoxImg = textBox.GetComponent<Image>();
        Color charColor = characterName.color;
        Color textColor = dialogueText.color;
        Color boxColor = textBoxImg.color;
        float dur = 0.75f;
        for (float i = 0; i <= dur; i += Time.deltaTime)
        {
            if (isInverted)
            {
                charColor.a = Mathf.Lerp(0, 1, (dur - i) / dur);
                textColor.a = Mathf.Lerp(0, 1, (dur - i) / dur);
                boxColor.a = Mathf.Lerp(0, boxColorAlpha, (dur - i) / dur);
            }
            else
            {

                charColor.a = Mathf.Lerp(0, 1, i / dur);
                textColor.a = Mathf.Lerp(0, 1, i / dur);
                boxColor.a = Mathf.Lerp(0, boxColorAlpha, i / dur);
            }

            characterName.color = charColor;
            dialogueText.color = textColor;
            textBoxImg.color = boxColor;
            yield return null;
        }
        if (isInverted)
            textBox.SetActive(false);
    }
    IEnumerator DisplayMessage(DialogueData dialogue)
    {
        characterName.text = dialogue.characterData.characterName;
        characterName.color = dialogue.characterData.nameColor;
        dialogueText.color = dialogue.characterData.dialogueColor;

        string invisTag = "<alpha=#00>";
        string text = dialogue.dialogueText;
        ParseVariables(ref text);
        dialogueText.text = text;
        isMessageRunning = true;
        for (int i = 0; i < text.Length; i++)
        {
            while (ParseTag(ref i, ref text)) ;
            if (tagWait != -1)
                yield return new WaitForSeconds(tagWait);
            string splicedText = text.Substring(0, i + 1) + invisTag + text.Substring(i + 1);
            dialogueText.text = splicedText;
            if (isMessageRunning)
            {
                yield return StartCoroutine(WaitForPunctuation(text[i]));
            }

        }
        isMessageRunning = false;
    }

    void ParseVariables(ref string text)
    {
        while (text.Contains("["))
        {
            int indexStart = text.IndexOf("[");
            int indexFinish = text.IndexOf("]");
            string bareVariable = text.Substring(indexStart + 1, indexFinish - indexStart - 1);
            text = text.Remove(indexStart, indexFinish - indexStart);
            text = text.Substring(0, indexStart) + QueryPlayerStatsForVariable(bareVariable) + text.Substring(indexStart + 1);
        }
    }

    string QueryPlayerStatsForVariable(string variable)
    {
        if (PlayerPrefs.HasKey(variable.ToString()))
        {
            return PlayerPrefs.GetInt(variable.ToString()).ToString();
        }
        return "0";
    }
    bool ParseTag(ref int index, ref string text)
    {
        if (text[index] == '<')
        {
            string fullTag = text.Substring(index);
            int closingIndex = fullTag.IndexOf('>');
            index += closingIndex + 1;
            fullTag = fullTag.Substring(0, closingIndex + 1);

            //at this point dupl has the full tag, complete with <>
            string bareTag = fullTag.Substring(1, fullTag.Length - 2);

            if (bareTag.Contains("speed"))
            {
                index -= closingIndex + 1;
                text = text.Remove(index, closingIndex + 1);
                if (bareTag.Contains("/"))
                {
                    speed = baseSpeed;
                    return true;
                }
                int equalIndex = bareTag.IndexOf("=");
                //in case the tag is just 'speed', return
                if (equalIndex == -1)
                {
                    return true;
                }
                //in case the tag is just 'speed=' also return
                if (equalIndex == bareTag.Length - 1)
                {
                    return true;
                }
                float val;
                try
                {
                    val = float.Parse(bareTag.Substring(equalIndex + 1));
                    speed = val;
                    return true;

                }
                catch (Exception e)
                {
                    return true;
                }

            }
            else if (bareTag.Contains("punch"))
            {
                index -= closingIndex + 1;
                text = text.Remove(index, closingIndex + 1);
                StartCoroutine(Punch());
                tagWait = 0.5f;
                return true;
            }
            //currentText += fullTag;
            return true;
        }
        return false;
    }

    IEnumerator WaitForPunctuation(char letter)
    {
        switch (letter)
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
                    yield return new WaitForSeconds(1.0f / speed);
                    break;
                }
        }
    }
    IEnumerator Punch()
    {
        Vector3 originalPosCam = mainCamera.transform.position;
        Vector3 originalPosImage = characterImage.transform.position;
        Vector3 originalPosTextbox = textBox.transform.position;
        float shakeAmount = 0.7f;
        for (float i = 0; i < 0.5f; i += Time.deltaTime)
        {
            Vector3 randPos = UnityEngine.Random.insideUnitSphere;
            mainCamera.transform.position = originalPosCam + randPos * shakeAmount;
            characterImage.transform.position = originalPosImage + randPos * shakeAmount;
            textBox.transform.position = originalPosTextbox + randPos * shakeAmount;
            yield return null;
        }
        mainCamera.transform.position = originalPosCam;
        characterImage.transform.position = originalPosImage;
        textBox.transform.position = originalPosTextbox;
        tagWait = -1;
    }
    IEnumerator FadeAndSlideCharacter(bool isInverted)
    {
        characterImage.gameObject.SetActive(true);
        Color c = new Color(1, 1, 1, 0);

        float shiftValue = 10.0f;
        //the image is now transparent and shifted;
        characterImage.color = c;
        characterImage.rectTransform.anchoredPosition += Vector2.left * shiftValue;

        Vector2 initialPos = characterImage.rectTransform.anchoredPosition;
        for (float i = 0; i < 0.75f; i += Time.deltaTime)
        {
            if (!isInverted)
                c.a = Mathf.Lerp(0.0f, 1.0f, i / 0.75f);
            else
                c.a = Mathf.Lerp(0.0f, 1.0f, (0.75f - i) / 0.75f);
            characterImage.color = c;
            if (!isInverted)
                characterImage.rectTransform.anchoredPosition =
                Vector2.Lerp(initialPos, initialPos + Vector2.right * shiftValue, i / 0.75f);
            else
                characterImage.rectTransform.anchoredPosition =
                Vector2.Lerp(initialPos, initialPos + Vector2.right * shiftValue, (0.75f - i) / 0.75f);

            yield return null;
        }
        if (isInverted)
            characterImage.gameObject.SetActive(false);
        yield return null;
    }
    IEnumerator BlurBackground(bool isInverted)
    {
        backgroundBlur.gameObject.SetActive(true);
        Color c = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        for (float i = 0; i <= 0.75f; i += Time.deltaTime)
        {
            if (!isInverted)
                c.a = Mathf.Lerp(0.0f, 0.4f, i / 0.75f);
            else
                c.a = Mathf.Lerp(0.0f, 0.4f, (0.75f - i) / 0.75f);
            backgroundBlur.color = c;
            yield return null;
        }
        if (isInverted)
            backgroundBlur.gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isMessageRunning)
            {
                isMessageRunning = false;
            }
            else
            {
                if (waitingForInput)
                    waitingForInput = false;
            }
        }
    }


}
*/
