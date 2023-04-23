using JetBrains.Annotations;
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
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    [Header("Text Properties")]
    [SerializeField]
    private TextMeshProUGUI characterName;
    [SerializeField]
    private TextMeshProUGUI dialogueText;
    [SerializeField]
    private TextMeshProUGUI narratorText;
    [SerializeField]
    private TextMeshProUGUI hiddenNarratorText;
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
    private AudioSource backgroundAudioSource;

    [Header("Option Properties")]
    [SerializeField]
    Button optionButton;
    [SerializeField]
    GameObject optionCanvas;
    [SerializeField]
    Slider volumeSlider;
    [SerializeField]
    Button leftButton;
    [SerializeField]
    Button rightButton;
    [SerializeField]
    TextMeshProUGUI speedText;

    [SerializeField]
    List<GameObject> characterImages;

    Dictionary<string, GameObject> currentImages;
    List<string> currentNarratorMessages;
    int currNarratorMessageIndex = 0;
    CharacterData charData = null;

    TextSpeed textSpeed = TextSpeed.NORMAL;
    bool isMessageRunning = false;
    bool isAtEndOfLine = false;
    bool waitingForInput = false;
    bool isPaused = false;

    public float speed = 20.0f;
    public float baseSpeed= 20.0f;
    public string currentText = "";
    public float tagWait = -1;

    float boxColorAlpha = 0.0f;
    Vector2 offMaxCharacter;
    Vector2 offMaxNarrator;

    private int maxDialogueLines = -1;

    EventSystem es = null;

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
        SetUserSettings();
        currentImages = new Dictionary<string, GameObject>();
        boxColorAlpha = characterTextBox.GetComponent<Image>().color.a;
        characterTextBox.SetActive(false);
        narratorTextBox.SetActive(false);
        currentNarratorMessages = new List<string>();
        offMaxCharacter = dialogueText.rectTransform.offsetMax;
        offMaxNarrator = narratorText.rectTransform.offsetMax;
        volumeSlider.value = 1.0f;
        if(mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();
        if (es == null)
            es = FindObjectOfType<EventSystem>();

    }

    void SetUserSettings()
    {
        UserSettings settings = SceneManager.Instance.userSettings;
        if(settings != null)
        {
            baseSpeed = speed = settings.TextSpeed;
            characterTextBox.GetComponent<Image>().color = settings.TextBoxColor;
            narratorTextBox.GetComponent<Image>().color = settings.TextBoxColor;
            characterName.fontSize = settings.CharacterNameFontSize;
            narratorText.fontSize = dialogueText.fontSize = settings.DialogueFontSize;
            characterTextBox.GetComponent<RectTransform>().anchoredPosition = settings.CharacterTextBoxPosition;
            narratorTextBox.GetComponent<RectTransform>().anchoredPosition = settings.NarratorTextBoxPosition;
            characterTextBox.GetComponent<RectTransform>().sizeDelta = settings.CharacterTextBoxSize;
            narratorTextBox.GetComponent<RectTransform>().sizeDelta = settings.NarratorTextBoxSize;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(!optionCanvas.activeInHierarchy)
        if (Input.GetKeyDown(KeyCode.E) && !isPaused)
        {
            if (isMessageRunning)
            {
                if (!isAtEndOfLine)
                    isMessageRunning = false;
                else
                {
                    if(!charData.isNarrator)
                    {
                        StartCoroutine(ScrollTextBox(dialogueText));
                    }
                    else
                    {
                        StartCoroutine(ScrollTextBox(narratorText));
                    }
                    isAtEndOfLine = false;
                }
            }
            else
            {
                if (waitingForInput)
                {
                    speed = baseSpeed;
                    waitingForInput = false;
                }
            }
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(optionCanvas.activeInHierarchy == false)
            {
                isPaused = true;
                OpenOptionCanvas();
            }
            else
            {
                isPaused = false;
                CloseOptionCanvas();
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
                        yield return StartCoroutine(DisplayMessageCharacter(command));
                    while (waitingForInput)
                        yield return null;
                    yield return StartCoroutine(HideMessageBox(command));
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
        FadeToBlackPanel.gameObject.SetActive(true);
        switch (scene.exitTransition)
        {
            case TransitionTypes.NONE:
                {
                    break;
                }
            case TransitionTypes.FADE:
                {
                    Color c = FadeToBlackPanel.color;
                    float initVolume = backgroundAudioSource.volume;
                    for (float i = 0; i < scene.exitTransitionValue; i+= Time.deltaTime)
                    {
                        c.a = Mathf.Lerp(0, 1, i / scene.exitTransitionValue);
                        FadeToBlackPanel.color = c;
                        backgroundAudioSource.volume = Mathf.Lerp(0, initVolume, (scene.exitTransitionValue - i) / scene.exitTransitionValue);
                        yield return null;
                    }
                    break;
                }
            case TransitionTypes.PUNCH:
                {
                    yield return StartCoroutine(Punch(0.5f, scene.exitTransitionValue));
                    break;
                }
        }
        List<string> keys = currentImages.Keys.ToList();
        foreach (var key in keys)
        {
            Destroy(currentImages[key]);
            currentImages.Remove(key);
        }
        characterTextBox.SetActive(false);
        characterName.text = string.Empty;
        dialogueText.text = string.Empty;
        backgroundAudioSource.Stop();
        backgroundAudioSource.clip = null;
        backgroundAudioSource.volume = 1.0f;
        narratorTextBox.SetActive(false);
        narratorText.text = "\t";

    }
    public IEnumerator LoadScene(SceneEditor scene)
    {
        backgroundAudioSource.volume = volumeSlider.value / 10.0f;
        if(scene.backgroundImage != null)
            backgroundImage.sprite = scene.backgroundImage;
        if(scene.backgroundMusic != null)
        {
            backgroundAudioSource.clip = scene.backgroundMusic;
            backgroundAudioSource.Play();
        }
        switch(scene.entryTransition)
        {
            case TransitionTypes.NONE:
                yield return null;
                break;
            case TransitionTypes.FADE:
                StartCoroutine(FadeBackgroundMusic(scene.entryTransitionValue));
                yield return StartCoroutine(FadeBackground(scene.entryTransitionValue));
                break;
            case TransitionTypes.PUNCH:
                yield return StartCoroutine(Punch(0.5f, scene.entryTransitionValue));
                break;
        }
        FadeToBlackPanel.gameObject.SetActive(false);
        yield return null;
    }
    public IEnumerator FadeBackgroundMusic(float time)
    {
        for(float i = 0; i < time; i+= Time.deltaTime)
        {
            backgroundAudioSource.volume = Mathf.Lerp(0, 1, i / time);
            yield return null;
        }
    }
    public IEnumerator FadeBackground(float time, bool _in = true)
    {
        Color c;
        if (_in)
            c = new Color(0, 0, 0, 0);
        else
            c = new Color(0, 0, 0, 1);
        FadeToBlackPanel.color = c;
        for(float i = 0.0f; i < time; i += Time.deltaTime)
        {
            if (!_in)
                c.a = Mathf.Lerp(0, 1, i / time);
            else
                c.a = Mathf.Lerp(0, 1, (time - i) / time);
            FadeToBlackPanel.color = c;
            yield return null;
        }
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
            else if(currentImages.Count == 3)
            {
                Debug.LogError($"Can't show more than 3 characters on screen at any given time!");
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
        else
        {
            if(!currentImages.ContainsKey(dialogue.Character.characterName))
            {
                Debug.LogError($"Can't hide {dialogue.Character.characterName} because he's not on the screen!");
                yield break;
            }
            var go = currentImages[dialogue.Character.characterName];
            Image currImg = go.GetComponent<Image>();
            currImg.color = new Color(1, 1, 1, 1);
            switch (dialogue.TransitionType)
            {
                case TransitionTypes.NONE:
                    {
                        currImg.color = new Color(1, 1, 1, 0);
                        break;
                    }
                case TransitionTypes.FADE:
                    {
                        Color c = currImg.color;
                        for(float i = 0; i < dialogue.Time; i += Time.deltaTime)
                        {
                            c.a = Mathf.Lerp(0.0f, 1.0f, (dialogue.Time - i) / dialogue.Time);
                            currImg.color = c;
                            yield return null;
                        }
                        break;
                    }
                case TransitionTypes.PUNCH:
                    {
                        StartCoroutine(Punch(0.5f, dialogue.Time));
                        currImg.color = new Color(1, 1, 1, 0);
                        yield return new WaitForSeconds(dialogue.Time);
                        break;
                    }
            }
            
            currentImages.Remove(dialogue.Character.characterName);
            Destroy(go);


        }
    }
    public IEnumerator HideMessageBox(CommandData dialogue)
    {
        //if 'Hide' is ticked
        if (dialogue.IsShow)
        {
            switch(dialogue.TransitionType)
            {
                case TransitionTypes.NONE:
                    {
                        yield return StartCoroutine(HideTextBox(dialogue.Character.isNarrator, 0));
                        break;
                    }
                case TransitionTypes.FADE:
                    {
                        yield return StartCoroutine(HideTextBox(dialogue.Character.isNarrator, dialogue.Time));
                        break;
                    }
                case TransitionTypes.PUNCH:
                    {
                        yield return StartCoroutine(HideTextBox(dialogue.Character.isNarrator, 0));
                        yield return StartCoroutine(Punch(0.5f, dialogue.Time));
                        break;
                    }
            }
        }
        else
            yield break;
        yield return null;
    }
    public IEnumerator HideTextBox(bool isNarrator, float time)
    {
        Image currTextBox = null;
        TextMeshProUGUI currText = null;
        if(isNarrator)
        {
            currTextBox = narratorTextBox.GetComponent<Image>();
            currText = narratorText;
        }
        else
        {
            currTextBox = characterTextBox.GetComponent<Image>();
            currText = dialogueText;
        }

        Color textBoxBGColor = currTextBox.color;
        float textBoxBGInitialAlpha = textBoxBGColor.a;
        Color textColor = currText.color;

        for(float i = 0; i <= time; i+= Time.deltaTime)
        {
            textBoxBGColor.a = Mathf.Lerp(0, 1, (time - i) / time);
            textColor.a = Mathf.Lerp(0, 1, (time - i) / time);
            currTextBox.color = textBoxBGColor;
            currText.color = textColor;
            yield return null;
        }
        currText.text = "";
        currTextBox.gameObject.SetActive(false);
        textBoxBGColor.a = textBoxBGInitialAlpha;
        currTextBox.color = textBoxBGColor;
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
        //Extract custom tags from the text and leave the rich text ones intact
        List<Tag> tags = ParseTextTags(ref text);
        int currTagIndex = -1;
        if (tags.Count > 0)
            currTagIndex = 0;
        
        var words = text.Split(' ');
        int currWordIndex = 0;
        text = words[0];

        isMessageRunning = true;
        string initialText = narratorText.text;
        //Narrator text is always considered to be paragraphs, so it's auto-indented
        if (!dialogue.Refresh)
        {
            //the narrator text box always reinitialises to an empty tab
            if (initialText != "\t")
            {
                initialText = initialText.Remove(initialText.Length - invisTag.Length - 2);
                initialText += "\n\t";
            }
            else
            {
                hiddenNarratorText.text = "\t" + dialogue.dialogueText;
                hiddenNarratorText.ForceMeshUpdate();
            }
        }
        else
        {
            narratorText.rectTransform.offsetMax = offMaxNarrator;
            initialText = "\t";
            currentNarratorMessages.Clear();
            currNarratorMessageIndex = 0;
            hiddenNarratorText.text = "\t" + dialogue.dialogueText;
            hiddenNarratorText.ForceMeshUpdate();
        }
        currentNarratorMessages.Add(dialogue.dialogueText);

        for (int i = 0; i < text.Length; i++)
        {
            while (waitingForInput || isPaused)
                yield return null;
            if (currTagIndex != -1)
            {
                while(currTagIndex != -1 && tags[currTagIndex].startIndex == i)
                    ParseTag(tags, ref currTagIndex, ref text, ref i);
            }
            if (tagWait != -1)
                yield return new WaitForSeconds(tagWait);
            if (i >= text.Length)
                break;
            string splicedText = text.Substring(0, i + 1) + invisTag + text.Substring(i + 1);
            narratorText.text = initialText + splicedText;
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
                    string wordToTest = StripTag(words[currWordIndex]);
                    if (text[i] != ' ')
                        text += $" {wordToTest}";
                    else
                        text += wordToTest;
                    //test if the text would be overflowing with the arrow
                    narratorText.text = initialText + text;
                    narratorText.text += "\u25BC";
                    narratorText.ForceMeshUpdate();
                    if (narratorText.isTextOverflowing)
                    {
                        //if text is overflowing, we remove the word without the tags
                        text = text.Remove(text.Length - wordToTest.Length);
                        //update the current word index
                        currWordIndex--;
                        //update the mesh and add the input signaller at the end
                        narratorText.text = initialText + text.TrimEnd();
                        narratorText.text += "\u25BC";
                        //set all the variable to receive input
                        waitingForInput = true;
                        isAtEndOfLine = true;
                        isMessageRunning = true;
                    }
                    else 
                    {
                        //we remove the input signaller we added by default
                        if (currWordIndex != words.Length - 1)
                        narratorText.text = narratorText.text.Remove(narratorText.text.Length - 1);
                        //if the word we wanted to add had tags inside, we remove the word without tags and add the tags in
                        if(wordToTest != words[currWordIndex])
                        {
                            text = text.Remove(text.Length - wordToTest.Length);
                            text += words[currWordIndex];
                            narratorText.text = initialText + text;
                        }
                        
                    }
                }
            }
        }
        narratorText.text = narratorText.text.Insert(narratorText.text.Length - invisTag.Length, "\u25BC");
        waitingForInput = true;
        isAtEndOfLine = false;
        isMessageRunning = false;

        yield return null;
    }
    public IEnumerator DisplayMessageCharacter(CommandData dialogue)
    {
        characterName.text = dialogue.Character.characterName;
        characterName.color = dialogue.Character.nameColor;
        dialogueText.color = dialogue.Character.dialogueColor;
        charData = dialogue.Character;
        if (narratorTextBox.gameObject.activeInHierarchy)
            narratorTextBox.SetActive(false);
        if (!characterTextBox.gameObject.activeInHierarchy)
            characterTextBox.SetActive(true);

        dialogueText.rectTransform.offsetMax = offMaxCharacter;
        if (currentImages.Count > 1 && currentImages.ContainsKey(characterName.text))
        {
            foreach(var kvp in currentImages)
            {
                if (kvp.Key != characterName.text)
                    kvp.Value.GetComponent<Image>().color = Color.grey;
                else
                    kvp.Value.GetComponent<Image>().color = Color.white;
            }
        }

        string invisTag = "<alpha=#00>";
        string text = dialogue.dialogueText;

        List<Tag> tags = ParseTextTags(ref text);
        int currTagIndex = -1;
        if (tags.Count > 0)
            currTagIndex = 0;

        var words = text.Split(' ');
        int currWordIndex = 0;
        text = words[0];
        isMessageRunning = true;

        for (int i = 0; i < text.Length; i++)
        {
            while (waitingForInput || isPaused)
                yield return null;
            if (currTagIndex != -1)
            {
                while (currTagIndex != -1 && tags[currTagIndex].startIndex == i)
                    ParseTag(tags, ref currTagIndex, ref text, ref i);
            }
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
                    string wordToTest = StripTag(words[currWordIndex]);
                    if (text[i] != ' ')
                        text += $" {wordToTest}";
                    else
                        text += wordToTest;
                    //test if the text would be overflowing with the arrow
                    dialogueText.text = text;
                    dialogueText.text += "\u25BC";
                    dialogueText.ForceMeshUpdate();
                    if (dialogueText.isTextOverflowing)
                    {
                        //if text is overflowing, we remove the word without the tags
                        text = text.Remove(text.Length - wordToTest.Length);
                        //update the current word index
                        currWordIndex--;
                        //update the mesh and add the input signaller at the end
                        dialogueText.text = text.TrimEnd();
                        dialogueText.text += "\u25BC";
                        //set all the variable to receive input
                        if (!dialogue.Refresh)
                        {
                            waitingForInput = true;
                            isAtEndOfLine = true;
                            isMessageRunning = true;
                        }
                        else
                            yield return StartCoroutine(ScrollTextBox(dialogueText, 1, Mathf.Lerp(0.05f, 0.15f,Mathf.Clamp(0, 1, (100-speed)/100))));
                    }
                    else
                    {
                        //we remove the input signaller we added by default
                        if (currWordIndex != words.Length - 1)
                            dialogueText.text = dialogueText.text.Remove(dialogueText.text.Length - 1);
                        //if the word we wanted to add had tags inside, we remove the word without tags and add the tags in
                        if (wordToTest != words[currWordIndex])
                        {
                            text = text.Remove(text.Length - wordToTest.Length);
                            text += words[currWordIndex];
                            dialogueText.text = text;
                        }

                    }
                }
            }
        }

        dialogueText.text = dialogueText.text.Insert(dialogueText.text.Length - invisTag.Length, "\u25BC");
        waitingForInput = true;
        isAtEndOfLine = false;
        isMessageRunning = false;
    }
    private IEnumerator ScrollTextBox(TextMeshProUGUI textBox, int lineOverload = -1, float timeOverload = 0.5f)
    {
        textBox.text = textBox.text.Remove(textBox.text.Length - 1);
        Vector2 maxOffset = textBox.rectTransform.offsetMax;
        float size = textBox.textInfo.lineInfo[0].ascender - textBox.textInfo.lineInfo[0].descender;
        int noOfLines;
        if(textBox == narratorText)
            noOfLines = hiddenNarratorText.textInfo.lineCount;
        else
        {
            if (maxDialogueLines == -1)
            {
                maxDialogueLines = textBox.textInfo.lineCount;
                noOfLines = maxDialogueLines - 1;
            }
            else
                noOfLines = maxDialogueLines - 1;
        }
        if (lineOverload != -1)
            noOfLines = lineOverload;
        Vector2 targetOffset = maxOffset + size * noOfLines * Vector2.up;
        float time = timeOverload;
        for(float i = 0; i < time; i+= Time.deltaTime)
        {
            textBox.rectTransform.offsetMax = Vector2.Lerp(maxOffset, targetOffset, i / time);
            yield return null;
        }
        textBox.rectTransform.offsetMax = targetOffset;
        //if we moved up the narrator's text box, update the first displayed paragraph
        if(textBox == narratorText)
        {
            currNarratorMessageIndex++;
            hiddenNarratorText.text = currentNarratorMessages[currNarratorMessageIndex];
        }
        waitingForInput = false;
        yield return null;
    }
    #endregion

    #region Text Parsing Functions

    string StripTag(string word)
    {
        if (!word.Contains("<"))
            return word;
        else
        {
            int startIndex = word.IndexOf("<");
            int closeIndex = word.LastIndexOf(">");
            word = word.Remove(startIndex, closeIndex - startIndex + 1);
            return word;
        }
    }
    public string[] ConflateTags(string[] words)
    {
        List<string> newWords = new List<string>();
        for (int i = 0; i < words.Length; i++)
        {
            if (!words[i].Contains("<"))
            {
                //word is simple, doesn't have the beginning of a tag
                newWords.Add(words[i]);
            }
            else
            {   //word has the beginning of a tag
                if (words[i].Contains(">"))
                {
                    //word is a self-enclosed tag e.g. <punch>, </speed>
                    newWords.Add(words[i]);
                }
                else
                {
                    //word is an incomplete tag e.g. <punch
                    string completeTag = words[i];
                    string bufferTag = "";
                    for (int j = i + 1; j < words.Length && j < i + 3; j++)
                    {
                        bufferTag += " " + words[j];
                        if (words[j].Contains(">"))
                        {
                            completeTag += bufferTag;
                            newWords.Add(completeTag);
                            i = j;
                            break;
                        }
                    }

                }
            }
        }

        return newWords.ToArray();
    }
    IEnumerator WaitForPunctuation(char letter)
    {
        float speedMult = 1.25f - ((int)textSpeed) * 0.25f;
        switch(letter)
        {
            case '!':
            case '?':
            case '.':
                {
                    yield return new WaitForSeconds(7.5f * speedMult / speed);
                    break;
                }
            case ',':
                {
                    yield return new WaitForSeconds(3.5f * speedMult / speed);
                    break;
                }
            default:
                {
                    yield return new WaitForSeconds(1.0f * speedMult / speed);
                    break;
                }
        }
    }
    void ParseTag(List<Tag> tags, ref int currTagIndex, ref string text, ref int index)
    {
        Tag t = tags[currTagIndex];
        if(t is PunchTag)
        {
            PunchTag pt = t as PunchTag;
            StartCoroutine(Punch(pt.strength, pt.time));
            tagWait = pt.time;
        }
        else if (t is SpeedTag)
        {
            SpeedTag st = t as SpeedTag;
            if(st.isClosing)
            {
                speed = baseSpeed;
            }
            else
            {
                speed = st.newSpeed;
            }
        }
        else
        {
            if(t.addToText)
            {
                index += t.fullTag.Length;
            }
        }
        currTagIndex++;
        if (currTagIndex >= tags.Count)
            currTagIndex = -1;
    }
    List<Tag> ParseTextTags(ref string text)
    {
        List<Tag> tags = new List<Tag>();
        int startIndex = text.IndexOf("<");
        int iter = 0;
        while(startIndex != -1 && iter < 1000)
        {
            int closeIndex = text.IndexOf(">", startIndex);
            iter++;
            bool toErase = true;
            if(closeIndex == -1)
            {
                Debug.LogError($"Error parsing tag in string {text.Substring(startIndex, startIndex + 10 > text.Length ? text.Length - startIndex : 10)}");
                return null;
            }
            string fullTag = text.Substring(startIndex, closeIndex - startIndex + 1);
            string bareTag = fullTag.Substring(1, fullTag.Length - 2);
            if(bareTag.StartsWith("speed"))
            {
                SpeedTag st = new SpeedTag();
                st.startIndex = startIndex;
                st.baseSpeed = baseSpeed;
                int equalIndex = bareTag.IndexOf("=");
                if (equalIndex != -1 && equalIndex != bareTag.Length - 1)
                {
                    string param = bareTag.Substring(equalIndex + 1);
                    try
                    {
                        float newSpeed = float.Parse(param);
                        st.newSpeed = newSpeed;
                    }
                    catch (Exception e)
                    {
                        ;
                    }
                }
                tags.Add(st);
            }
            else if (bareTag.StartsWith("/speed"))
            {
                SpeedTag st = new SpeedTag();
                st.startIndex = startIndex;
                st.isClosing = true;
                tags.Add(st);
            }
            else if (bareTag.StartsWith("punch"))
            {
                PunchTag pt = new PunchTag();
                pt.startIndex = startIndex; if (bareTag.Contains("strength="))
                {
                    try
                    {
                        int strIndex = bareTag.IndexOf("strength=");
                        int nextSpaceIndex = bareTag.IndexOf(" ", strIndex);
                        if (nextSpaceIndex == -1)
                            nextSpaceIndex = bareTag.Length;
                        int strLength = "strength=".Length;
                        string strSubstring = bareTag.Substring(strIndex + strLength, nextSpaceIndex - strIndex - strLength);
                        pt.strength = float.Parse(strSubstring);
                    }
                    catch (Exception e)
                    {
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
                        pt.time = float.Parse(timeSubstring);
                    }
                    catch (Exception e)
                    {
                    }
                }
                tags.Add(pt);
            }
            else
            {
                Tag t = new Tag();
                t.fullTag = fullTag;
                t.startIndex = startIndex;
                t.addToText = true;
                if (bareTag[0] == '/')
                    t.isClosing = true;
                tags.Add(t);
                toErase = false;
            }
            if (toErase)
            {
                text = text.Remove(startIndex, fullTag.Length);
                startIndex = text.IndexOf("<");
            }
            else
            {
                startIndex = text.IndexOf("<", startIndex + 1);
            }

        }
        return tags;
    }

    IEnumerator Punch(float strength, float time)
    {
        Vector3 originalPosBG = backgroundImage.transform.position;
        List<Vector3> originalPosImages = currentImages.Values.ToListPooled().Select(x => x.transform.position).ToList();
        Vector3 originalPosTextbox = characterTextBox.transform.position;
        Vector3 originalNarratorPos = narratorTextBox.transform.position;
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
            narratorTextBox.transform.position = originalNarratorPos + randPos * shakeAmount;
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
        narratorTextBox.transform.position = originalNarratorPos;
        tagWait = -1;
    }
    #endregion

    #region Option Canvas Functions

    void OpenOptionCanvas()
    {
        Time.timeScale = 0;
        optionCanvas.SetActive(true);
        OnOptionCanvasEnabled();
    }

    void CloseOptionCanvas()
    {
        Time.timeScale = 1;
        optionCanvas.SetActive(false);
    }
    public void OnOptionButtonClick()
    {
        optionButton.gameObject.SetActive(false);
        isPaused = true;
        OpenOptionCanvas();
    }

    public void OnCloseButtonClick()
    {
        isPaused = false;
        optionButton.gameObject.SetActive(true);
        CloseOptionCanvas();
    }

    void OnOptionCanvasEnabled()
    {
        speedText.text = textSpeed.ToString();
        leftButton.gameObject.SetActive(!(((int)textSpeed) == 0));
        rightButton.gameObject.SetActive(!(((int)textSpeed) == 2));
    }
    public void OnVolumeSliderChanged()
    {
        backgroundAudioSource.volume = volumeSlider.value / 10.0f;
    }
    public void OnRightButtonPressed()
    {
        if(((int)textSpeed) < 2)
        {
            textSpeed = (TextSpeed)(textSpeed + 1);
            OnOptionCanvasEnabled();
        }
    }

    public void OnLeftButtonPressed()
    {
        if((int)textSpeed > 0)
        {
            textSpeed = (TextSpeed)(textSpeed - 1);
            OnOptionCanvasEnabled();
        }
    }
    #endregion
}

public class Tag
{
    public string fullTag;
    public int startIndex;
    public bool isClosing = false;
    //tags that are added to the text are automatically skipped 
    public bool addToText = false;
}

public class PunchTag : Tag
{
    public float strength = 0.7f;
    public float time = 0.5f;
}

public class SpeedTag : Tag
{
    public float baseSpeed;
    public float newSpeed;

}

public enum TextSpeed
{
    SLOW,
    NORMAL,
    FAST
}
