using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
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
    private TextMeshProUGUI narratorText;
    [SerializeField]
    private GameObject narratorTextBox;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private GameObject characterTextBox;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private Image FadeToBlackPanel;

    [SerializeField]
    List<GameObject> characterImages;

    Dictionary<string, GameObject> currentImages;
    CharacterData charData = null;

    bool isMessageRunning = false;
    bool isAtEndOfLine = false;
    bool waitingForInput = false;

    public float speed = 20.0f;
    public float baseSpeed= 20.0f;
    public string currentText = "";
    public float tagWait = -1;

    float boxColorAlpha = 0.0f;
    Vector2 offMaxCharacter;
    Vector2 offMaxNarrator;

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
        currentImages = new Dictionary<string, GameObject>();
        speed = baseSpeed;
        boxColorAlpha = characterTextBox.GetComponent<Image>().color.a;
        characterTextBox.SetActive(false);
        narratorTextBox.SetActive(false);
        offMaxCharacter = dialogueText.rectTransform.offsetMax;
        offMaxNarrator = narratorText.rectTransform.offsetMax;
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
                if (!isAtEndOfLine)
                    isMessageRunning = false;
                else
                {
                    if(!charData.isNarrator)
                    {
                        StartCoroutine(MoveCharacterTextBox(2));
                        isAtEndOfLine = false;
                    }
                }
            }
            else
            {
                if (waitingForInput)
                {
                    dialogueText.rectTransform.offsetMax = offMaxCharacter;
                    speed = baseSpeed;
                    waitingForInput = false;
                }
            }
        }
    }
    public void PlayScene(SceneEditor scene)
    {
        StartCoroutine(PlaySceneCoroutine(scene));
    }
    private IEnumerator PlaySceneCoroutine(SceneEditor scene)
    {
        yield return StartCoroutine(LoadScene(scene));
        foreach(CommandData command in scene.Commands)
        {
            yield return StartCoroutine(ParseCommand(command));
        }

        yield return StartCoroutine(UnloadScene(scene));
        SceneManager.Instance.currScene = scene.linkedScene;
        SceneManager.Instance.isScenePlaying = false;


    }

    private IEnumerator ParseCommand(CommandData command)
    {
        switch(command.type)
        {
            case CommandType.SAY:
                {
                    if(command.Character.isNarrator)
                    {
                        yield return StartCoroutine(DisplayMessageNarrator(command));
                    }
                    else
                        yield return StartCoroutine(DisplayMessage(command));
                    while (waitingForInput)
                        yield return null;
                    break;
                }
            case CommandType.SHOW:
                {
                    yield return StartCoroutine(ShowCharacter(command));
                    break;
                }
            case CommandType.WAIT:
                {
                    yield return new WaitForSeconds(command.Time);
                    break;
                }
            case CommandType.MOVE:
                {
                    yield return StartCoroutine(MoveCharacter(command));
                    break;
                }
            case CommandType.SPRITE:
                {
                    yield return StartCoroutine(ChangeSprite(command));
                    break;
                }
        }
    }
    public IEnumerator UnloadScene(SceneEditor scene)
    {
        switch(scene.exitTransition)
        {
            case TransitionTypes.NONE:
                {
                    backgroundImage.gameObject.SetActive(false);
                    List<string> keys = currentImages.Keys.ToList();
                    foreach(var key in keys)
                    {
                        Destroy(currentImages[key]);
                        currentImages.Remove(key);
                    }
                    characterTextBox.SetActive(false);
                    characterName.text = string.Empty;
                    dialogueText.text = string.Empty;
                    break;
                }
            case TransitionTypes.FADE:
                {
                    Color c = FadeToBlackPanel.color;
                    FadeToBlackPanel.gameObject.SetActive(true);
                    for (float i = 0; i < scene.exitTransitionValue; i+= Time.deltaTime)
                    {
                        c.a = Mathf.Lerp(0, 1, i / scene.exitTransitionValue);
                        FadeToBlackPanel.color = c;
                        yield return null;
                    }
                    backgroundImage.gameObject.SetActive(false);
                    List<string> keys = currentImages.Keys.ToList();
                    foreach (var key in keys)
                    {
                        Destroy(currentImages[key]);
                        currentImages.Remove(key);
                    }
                    characterTextBox.SetActive(false);
                    characterName.text = string.Empty;
                    dialogueText.text = string.Empty;
                    break;
                }
        }
    }

    public IEnumerator LoadScene(SceneEditor scene)
    {
        backgroundImage.sprite = scene.backgroundImage;
        FadeToBlackPanel.gameObject.SetActive(false);
        switch(scene.entryTransition)
        {
            case TransitionTypes.NONE:
                backgroundImage.gameObject.SetActive(true);
                yield return null;
                break;
            case TransitionTypes.FADE:
                yield return StartCoroutine(FadeBackground(scene.entryTransitionValue));
                break;
        }
        yield return null;
    }
    public IEnumerator FadeBackground(float time, bool _in = true)
    {
        Color c;
        if (_in)
            c = new Color(1, 1, 1, 0);
        else
            c = new Color(1, 1, 1, 1);
        backgroundImage.color = c;
        if(_in)
            backgroundImage.gameObject.SetActive(true);
        for(float i = 0.0f; i < time; i += Time.deltaTime)
        {
            if (_in)
                c.a = Mathf.Lerp(0, 1, i / time);
            else
                c.a = Mathf.Lerp(0, 1, (time - i) / time);
            backgroundImage.color = c;
            yield return null;
        }
        if(!_in)
            backgroundImage.gameObject.SetActive(false);
    }

    #region Command Parsing

    public IEnumerator ChangeSprite(CommandData command)
    {
        if (command.Character.isNarrator)
            yield break;
        if(currentImages.ContainsKey(command.Character.characterName))
        {
            Image img = currentImages[command.Character.characterName].GetComponent<Image>();
            var currEmotion = SelectSpriteFromCommand(command);
            img.sprite = currEmotion;
        }
        else
        {
            Debug.LogError($"Cannot change sprite of character {command.Character.characterName} because he isn't in the scene!" +
                $"Make sure every SPRITE command is preceded by a SHOW command or that you haven't made the sprite disappear with a SHOW - Hide command!");
            yield break;
        }
        yield return null;
    }
    public IEnumerator MoveCharacter(CommandData command)
    {
        if (command.Character.isNarrator)
            yield break;
        var key = currentImages.Keys.ToList().FirstOrDefault(x => x == command.Character.characterName);
        if (key == null)
        {
            Debug.LogError($"Cannot change sprite of character {command.Character.characterName} because he isn't in the scene!" +
                $"Make sure every MOVE command is preceded by a SHOW command and that you haven't made the sprite disappear with a SHOW - Hide command!");
            yield break;
        }
        //In this case it means 'Text box should be hidden'
        if(command.IsShow)
        {
            characterTextBox.SetActive(false);
        }
        Image img = currentImages[key].GetComponent<Image>();
        Vector3 posFrom = img.rectTransform.anchoredPosition;
        Vector3 posTo = characterImages[(int)command.LocationTo].GetComponent<Image>().rectTransform.anchoredPosition;
        Vector3 currPos = posFrom;
        for (float i = 0; i < command.Time; i += Time.deltaTime)
        {
            currPos = Vector3.Lerp(posFrom, posTo, i / command.Time);
            img.rectTransform.anchoredPosition = currPos;
            yield return null;
        }
    }
    private Sprite SelectSpriteFromCommand(CommandData dialogue)
    {
        Sprite emotionSprite = null;
        if (dialogue.emotion == "Default")
        {
            emotionSprite = dialogue.Character.characterImage;
        }
        else
        {
            EmotionPair emotion = dialogue.Character.emotions.First(x => x.emotion == dialogue.emotion);
            emotionSprite = emotion.sprite;
        }
        return emotionSprite;
    }
    private IEnumerator ShowCharacter(CommandData dialogue)
    {
        if (dialogue.Character.isNarrator)
            yield break;
        if(!dialogue.IsShow)
        {
            if(currentImages.ContainsKey(dialogue.Character.characterName))
            {
                Debug.LogError($"Character {dialogue.Character.characterName} is already on screen!");
                yield break;
            }
            else
            {
                if(dialogue.Character.name != "")
                {

                    var go = Instantiate(characterImages[(int)dialogue.LocationTo], canvas.transform);
                    go.name = dialogue.Character.characterName;
                    go.transform.SetSiblingIndex(1);
                    currentImages.Add(dialogue.Character.characterName, go);
                    Image currImg = go.GetComponent<Image>();
                    currImg.color = new Color(1, 1, 1, 0);
                    currImg.sprite = SelectSpriteFromCommand(dialogue);
                    switch (dialogue.TransitionType)
                    {
                        case TransitionTypes.NONE:
                            {
                                currImg.color = new Color(1, 1, 1, 1);
                                yield return null;
                                break;
                            }
                        case TransitionTypes.FADE:
                            {
                                Color c = currImg.color;
                                for (float i = 0; i < dialogue.Time; i += Time.deltaTime)
                                {
                                    c.a = Mathf.Lerp(0.0f, 1.0f, i / dialogue.Time);
                                    currImg.color = c;
                                    yield return null;
                                }
                                break;
                            }
                        case TransitionTypes.PUNCH:
                            {
                                currImg.color = new Color(1, 1, 1, 1);
                                yield return StartCoroutine(Punch(0.5f, dialogue.Time));
                                yield return null;
                                break;
                            }
                    }
                }
            }

        }
    }
    public IEnumerator DisplayMessageNarrator(CommandData dialogue)
    {
        narratorText.color = dialogue.Character.dialogueColor;
        charData = dialogue.Character;

        if (!narratorTextBox.gameObject.activeInHierarchy)
            narratorTextBox.SetActive(true);
        if (characterTextBox.gameObject.activeInHierarchy)
            characterTextBox.SetActive(true);

        string invisTag = "<alpha=#00>";
        string text = dialogue.dialogueText;
        var words = text.Split(' ');
        int currWordIndex = 0;
        //narrator text is considered to always be written in paragraphs
        //narrator text is also expected to consist only of *one* paragraph
        //therefore it's indented at the beginning
        text = "\t" + words[0];

        isMessageRunning = true;
        if (dialogue.IsShow)
            narratorText.text += "\n";
        for (int i = 0; i < text.Length; i++)
        {
            while (waitingForInput)
                yield return null;
            while (ParseTag(ref i, ref text, ref currWordIndex, words)) ;
            if (tagWait != -1)
                yield return new WaitForSeconds(tagWait);
            if (i >= text.Length)
                break;
            string splicedText = text.Substring(0, i + 1) + invisTag + text.Substring(i + 1);
            narratorText.text = splicedText;
            if (isMessageRunning)
            {
                yield return StartCoroutine(WaitForPunctuation(text[i]));
            }
            if (i == text.Length - 1)
            {
                if (currWordIndex < words.Length - 1)
                {
                    //add the next word in
                    currWordIndex++;
                    if (text[i] != ' ')
                        text += $" {words[currWordIndex]}";
                    else
                        text += words[currWordIndex];
                    //test if the text would be overflowing with the arrow
                    narratorText.text = text;
                    narratorText.text += "\u25BC";
                    narratorText.ForceMeshUpdate();
                }
                if (narratorText.isTextOverflowing)
                {
                    text = text.Remove(text.Length - words[currWordIndex].Length);
                    currWordIndex--;
                    narratorText.text = text;
                    narratorText.text += "\u25BC";
                    waitingForInput = true;
                    isAtEndOfLine = true;
                    isMessageRunning = true;
                }
                else if (currWordIndex != words.Length - 1)
                {
                    narratorText.text = narratorText.text.Remove(narratorText.text.Length - 1);
                }
            }
        }
        narratorText.text = narratorText.text.Insert(narratorText.text.Length - invisTag.Length, "\u25BC");
        waitingForInput = true;
        isAtEndOfLine = false;
        isMessageRunning = false;

        yield return null;
    }
    public IEnumerator DisplayMessage(CommandData dialogue)
    {
        characterName.text = dialogue.Character.characterName;
        characterName.color = dialogue.Character.nameColor;
        dialogueText.color = dialogue.Character.dialogueColor;
        charData = dialogue.Character;
        if (narratorTextBox.gameObject.activeInHierarchy)
            narratorTextBox.SetActive(false);
        if (!characterTextBox.gameObject.activeInHierarchy)
            characterTextBox.SetActive(true);

        string invisTag = "<alpha=#00>";
        string text = dialogue.dialogueText;
        var words = text.Split(' ');
        int currWordIndex = 0;
        text = words[0];
        isMessageRunning = true;
        for (int i = 0; i < text.Length; i++)
        {
            while (waitingForInput)
                yield return null;
            while (ParseTag(ref i, ref text, ref currWordIndex, words));
            if (tagWait != -1)
                yield return new WaitForSeconds(tagWait);
            if (i >= text.Length)
                break;
            string splicedText = text.Substring(0, i + 1) + invisTag + text.Substring(i + 1);
            dialogueText.text = splicedText;  
            if (isMessageRunning)
            {
                yield return StartCoroutine(WaitForPunctuation(text[i]));
            }
            if (i == text.Length - 1)
            {
                if (currWordIndex < words.Length - 1)
                {
                    //add the next word in
                    currWordIndex++;
                    if (text[i] != ' ')
                        text += $" {words[currWordIndex]}";
                    else
                        text += words[currWordIndex];
                    //test if the text would be overflowing with the arrow
                    dialogueText.text = text;
                    dialogueText.text += "\u25BC";
                    dialogueText.ForceMeshUpdate();
                }
                if (dialogueText.isTextOverflowing)
                {
                    text = text.Remove(text.Length - words[currWordIndex].Length);
                    currWordIndex--;
                    dialogueText.text = text;
                    dialogueText.text += "\u25BC";
                    waitingForInput = true;
                    isAtEndOfLine = true;
                    isMessageRunning = true;
                }
                else if (currWordIndex != words.Length - 1)
                {
                    dialogueText.text = dialogueText.text.Remove(dialogueText.text.Length - 1);
                }
                else
                    dialogueText.text += "\u25BC";
            }
        }
        dialogueText.text += "\u25BC";
        waitingForInput = true;
        isAtEndOfLine = false;
        isMessageRunning = false;
    }

    private IEnumerator MoveCharacterTextBox(int noOfLines)
    {
        dialogueText.text = dialogueText.text.Remove(dialogueText.text.Length - 1);
        Vector2 maxOffset = dialogueText.rectTransform.offsetMax;
        Vector2 targetOffset = maxOffset + 29f * noOfLines * Vector2.up;
        float time = 0.5f;
        for(float i = 0; i < time; i+= Time.deltaTime)
        {
            dialogueText.rectTransform.offsetMax = Vector2.Lerp(maxOffset, targetOffset, i / time);
            yield return null;
        }
        dialogueText.rectTransform.offsetMax = targetOffset;
        waitingForInput = false;
        yield return null;
    }
    #endregion

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
                    yield return new WaitForSeconds(1.0f/speed);
                    break;
                }
        }
    }

    bool ParseTag(ref int index, ref string text, ref int currWordIndex, string[] words)
    {
        if (index >= text.Length)
            return false;
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
                if(fullTag == words[currWordIndex])
                {
                    currWordIndex++;
                    if (currWordIndex < words.Length)
                        text += words[currWordIndex];

                }
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
                float strength = 10.0f, time = 0.5f;
                if(bareTag.Contains("strength="))
                {
                    try
                    {
                        int strIndex = bareTag.IndexOf("strength=");
                        int nextSpaceIndex = bareTag.IndexOf(" ", strIndex);
                        if (nextSpaceIndex == -1)
                            nextSpaceIndex = bareTag.Length;
                        int strLength = "strength=".Length;
                        string strSubstring = bareTag.Substring(strIndex + strLength, nextSpaceIndex - strIndex - strLength);
                        strength = float.Parse(strSubstring);
                    }
                    catch(Exception e)
                    {
                        strength = -1;
                    }
                }
                if (bareTag.Contains("time="))
                {
                    try
                    {
                        int timeIndex = bareTag.IndexOf("time=");
                        int nextSpaceIndex = bareTag.IndexOf(" ", timeIndex);
                        if (nextSpaceIndex == -1)
                            nextSpaceIndex = bareTag.Length;
                        int timeLength = "time=".Length;
                        string timeSubstring = bareTag.Substring(timeIndex + timeLength, nextSpaceIndex - timeIndex - timeLength);
                        time = float.Parse(timeSubstring);
                    }
                    catch (Exception e)
                    {
                        time = -1;
                    }
                }
                index -= closingIndex + 1;
                text = text.Remove(index, closingIndex + 1);
                if(strength == -1)
                {
                    StartCoroutine(Punch(0.7f, time));
                    tagWait = time;
                    return true;
                }
                else if(time == -1)
                {
                    StartCoroutine(Punch(strength, 0.5f));
                    tagWait = 0.5f;
                    return true;
                }
                else
                {
                    StartCoroutine(Punch(strength, time));
                    tagWait = time;
                    return true;
                }
            }
            //currentText += fullTag;
            return true;
        }
        return false;
    }
    IEnumerator Punch(float strength, float time)
    {
        Vector3 originalPosBG = backgroundImage.transform.position;
        List<Vector3> originalPosImages = currentImages.Values.ToListPooled().Select(x => x.transform.position).ToList();
        Vector3 originalPosTextbox = characterTextBox.transform.position;
        float shakeAmount = strength * 5;
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            Vector3 randPos = UnityEngine.Random.insideUnitSphere;
            backgroundImage.transform.position = originalPosBG + randPos * shakeAmount;
            int j = 0;
            foreach (var kvp in currentImages)
            {
                kvp.Value.transform.position = originalPosImages[j] + randPos * shakeAmount;
                j++;
            }
            characterTextBox.transform.position = originalPosTextbox + randPos * shakeAmount;
            yield return null;
        }
        backgroundImage.transform.position = originalPosBG;
        int k = 0;
        foreach (var kvp in currentImages)
        {
            kvp.Value.transform.position = originalPosImages[k];
            k++;
        }
        characterTextBox.transform.position = originalPosTextbox;
        tagWait = -1;
    }

    #endregion

    #region Transition Functions


    IEnumerator FadeTextBoxAway(bool isInverted)
    {
        characterTextBox.SetActive(true);
        Image textBoxImg = characterTextBox.GetComponent<Image>();
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
            characterTextBox.SetActive(false);
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
