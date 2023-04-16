using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEditor.Progress;
using static UnityEditor.Rendering.FilterWindow;

namespace UnityEditor
{

    public class DialogueListEditor
    {
        int highlightedIndex = -1;
        List<Rect> dialoguesRect = new List<Rect>();
        private Texture iconUp;
        private Texture iconDown;
        public int commandIndex = 0;
        public string[] commandOptions = Enum.GetNames(typeof(CommandType)).ToArray();
        List<CharacterData> characters;

        public DialogueListEditor()
        {
            characters = GetAllCharacters();
        }
        public void Show(SerializedObject obj, SerializedProperty list, Event e)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (list.arraySize == 0)
            {
                EditorGUILayout.LabelField(new GUIContent()
                {
                    text = "There are no dialogues"
                }, EditorStyles.label);
            }
            else
            {
                DrawBoxes(list);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal();
            DrawButtons(obj, list);
            ListenForEvents(list, e);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical();
            if (highlightedIndex != -1)
            {
                DrawHighlightedEditor(list);
            }
            EditorGUILayout.EndVertical();

        }


        private void DrawBoxes(SerializedProperty list)
        {
            GUIStyle CommandTypeLabelStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState() { textColor = Color.white }
            };
            GUIStyle CharacterStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() { textColor = Color.white }

            };
            GUIStyle DialogueStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Italic,
                normal = new GUIStyleState() { textColor = Color.white }
            };
            List<Rect> boxes = new List<Rect>();
            GUILayout.Space(4);
            for (int i = 0; i < list.arraySize; i++)
            {
                var commandElement = list.GetArrayElementAtIndex(i);
                var commandType = commandElement.FindPropertyRelative("type");
                CommandType type = (CommandType)commandType.enumValueIndex;
                var character = commandElement.FindPropertyRelative("Character");
                if(character.objectReferenceValue == null)
                {
                    character.objectReferenceValue = SearchForNarrator();
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                boxes.Add(EditorGUILayout.BeginHorizontal());
                GUILayout.Space(10);
                if (i == highlightedIndex)
                {
                    Rect last = boxes.Last();
                    last.x--;
                    last.y--;
                    last.width += 2;
                    last.height += 2;
                    EditorGUI.DrawRect(last, Color.green);
                }
                EditorGUI.DrawRect(boxes.Last(), Color.grey);
                EditorGUILayout.BeginHorizontal();
                ShowInsideData(type, list, i, CommandTypeLabelStyle, CharacterStyle, DialogueStyle);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
            GUILayout.Space(2);
            dialoguesRect = boxes;
        }
        #region Show Inside List Elements
        private void ShowInsideData(CommandType type, SerializedProperty list, int index, GUIStyle style1, GUIStyle style2, GUIStyle style3)
        {
            switch (type)
            {
                case CommandType.SAY:
                    ShowSayInside(list, index, style1, style2, style3);
                    break;

                case CommandType.MOVE:
                    ShowMoveInside(list, index, style1, style2, style3);
                    break;
                case CommandType.SHOW:
                    ShowShowInside(list, index, style1, style2, style3);
                    break;
                case CommandType.WAIT:
                    ShowWaitInside(list, index, style1, style2, style3);
                    break;
                case CommandType.SPRITE:
                    ShowSpriteInside(list, index, style1, style2, style3);
                    break;
            }
        }
        private void ShowSayInside(SerializedProperty list, int index, GUIStyle style1, GUIStyle style2, GUIStyle style3)
        {
            var characterProperty = list.GetArrayElementAtIndex(index).FindPropertyRelative("Character");
            var character = new SerializedObject(characterProperty.objectReferenceValue);

            EditorGUILayout.LabelField("SAY", style1, new[] { GUILayout.Width(30) });
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"{character.FindProperty("characterName").stringValue}", style2, new[] { GUILayout.Width(50) });
            string s = list.GetArrayElementAtIndex(index).FindPropertyRelative("dialogueText").stringValue;
            bool truncated = s.Length > 13;
            string toShow = "";
            if (truncated)
            {
                toShow = s.Substring(0, 13) + "...";
            }
            else toShow = s;
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(toShow, style3, new[] { GUILayout.Width(100) });
        }
        private void ShowMoveInside(SerializedProperty list, int index, GUIStyle style1, GUIStyle style2, GUIStyle style3)
        {
            var characterProperty = list.GetArrayElementAtIndex(index).FindPropertyRelative("Character");
            var character = new SerializedObject(characterProperty.objectReferenceValue);

            EditorGUILayout.LabelField("MOVE", style1, new[] { GUILayout.Width(30) });
            GUILayout.FlexibleSpace();
            if(characters.Count == 1)
            {
                EditorGUILayout.LabelField($"NONE", style2, new[] { GUILayout.Width(50) });
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("", style3, new[] { GUILayout.Width(100) });
                return;
            }
            else
            {
                if(character.FindProperty("characterName").stringValue == "Narrator")
                    characterProperty.objectReferenceValue = characters[1];
            }
            EditorGUILayout.LabelField($"{character.FindProperty("characterName").stringValue}", style2, new[] { GUILayout.Width(50) });

            GUILayout.FlexibleSpace();
            var toLocation = list.GetArrayElementAtIndex(index).FindPropertyRelative("LocationTo");
            var locationIndex = toLocation.enumValueIndex;
            var toLocationString = (ScreenPosition)locationIndex;
            EditorGUILayout.LabelField(toLocationString.ToString(), style3, new[] { GUILayout.Width(100) });
        }
        private void ShowShowInside(SerializedProperty list, int index, GUIStyle style1, GUIStyle style2, GUIStyle style3)
        {

            var characterProperty = list.GetArrayElementAtIndex(index).FindPropertyRelative("Character");
            var character = new SerializedObject(characterProperty.objectReferenceValue);

            var emotionProperty = list.GetArrayElementAtIndex(index).FindPropertyRelative("emotion");
            var emotionString = emotionProperty.stringValue;

            EditorGUILayout.LabelField("SHOW", style1, new[] { GUILayout.Width(30) });
            GUILayout.FlexibleSpace();
            if (characters.Count == 1)
            {
                EditorGUILayout.LabelField($"NONE", style2, new[] { GUILayout.Width(50) });
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("", style3, new[] { GUILayout.Width(100) });
                return;
            }
            else
            {
                if (character.FindProperty("characterName").stringValue == "Narrator")
                    characterProperty.objectReferenceValue = characters[1];
            }
            EditorGUILayout.LabelField($"{character.FindProperty("characterName").stringValue}({emotionString})", style2, new[] { GUILayout.Width(50) });

            GUILayout.FlexibleSpace();
            var position = list.GetArrayElementAtIndex(index).FindPropertyRelative("LocationTo");
            var positionIndex = position.enumValueIndex;
            var positionString = (ScreenPosition)positionIndex;
            EditorGUILayout.LabelField(positionString.ToString(), style3, new[] { GUILayout.Width(100) });
        }
        private void ShowWaitInside(SerializedProperty list, int index, GUIStyle style1, GUIStyle style2, GUIStyle style3)
        {
            EditorGUILayout.LabelField("WAIT", style1, new[] { GUILayout.Width(30) });
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"", style2, new[] { GUILayout.Width(50) });

            GUILayout.FlexibleSpace();
            var waitTime = list.GetArrayElementAtIndex(index).FindPropertyRelative("Time");

            EditorGUILayout.LabelField(waitTime.floatValue.ToString(), style3, new[] { GUILayout.Width(100) });
        }
        private void ShowSpriteInside(SerializedProperty list, int index, GUIStyle style1, GUIStyle style2, GUIStyle style3)
        {

            var characterProperty = list.GetArrayElementAtIndex(index).FindPropertyRelative("Character");
            var character = new SerializedObject(characterProperty.objectReferenceValue);

            var emotionProperty = list.GetArrayElementAtIndex(index).FindPropertyRelative("emotion");
            var emotion = emotionProperty.stringValue;

            EditorGUILayout.LabelField("SPRITE", style1, new[] { GUILayout.Width(30) });
            GUILayout.FlexibleSpace();
            if (characters.Count == 1)
            {
                EditorGUILayout.LabelField($"NONE", style2, new[] { GUILayout.Width(50) });
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("", style3, new[] { GUILayout.Width(100) });
                return;
            }
            else
            {
                if (character.FindProperty("characterName").stringValue == "Narrator")
                    characterProperty.objectReferenceValue = characters[1];
            }
            EditorGUILayout.LabelField($"{character.FindProperty("characterName").stringValue}", style2, new[] { GUILayout.Width(50) });

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField(emotion, style3, new[] { GUILayout.Width(100) });
        }
        #endregion
        private void DrawButtons(SerializedObject obj, SerializedProperty list)
        {
            if (GUILayout.Button(new GUIContent()
            {
                text = "↑"
            }, EditorStyles.miniButton))
            {
                if (highlightedIndex != -1 && highlightedIndex != 0 && list.arraySize > 1)
                {
                    list.MoveArrayElement(highlightedIndex, highlightedIndex - 1);
                    highlightedIndex--;
                }
            }
            if (GUILayout.Button(new GUIContent()
            {
                text = "↓"
            }, EditorStyles.miniButton))
            {
                if (highlightedIndex != -1 && highlightedIndex != list.arraySize - 1 && list.arraySize > 1)
                {

                    list.MoveArrayElement(highlightedIndex, highlightedIndex + 1);
                    highlightedIndex++;
                }
            }

            GUILayout.FlexibleSpace();
            commandIndex = EditorGUILayout.Popup(commandIndex, commandOptions);
            if (GUILayout.Button(new GUIContent()
            {
                text = "+"
            }, EditorStyles.miniButton))
            {
                list.arraySize++;
                SerializedProperty el;
                //if there is a highlighted command, add below it
                if(highlightedIndex != -1)
                {
                    for(int i = list.arraySize - 1; i > highlightedIndex + 1; i--)
                    {
                        list.MoveArrayElement(i - 1, i);
                    }
                    el = list.GetArrayElementAtIndex(highlightedIndex + 1);
                    highlightedIndex = highlightedIndex + 1;
                }
                //else add at the end
                else
                {
                    el = list.GetArrayElementAtIndex(list.arraySize - 1);
                    highlightedIndex = list.arraySize - 1;
                }
                SerializedProperty type = el.FindPropertyRelative("type");
                type.enumValueIndex = commandIndex;
                SerializedProperty character = el.FindPropertyRelative("Character");
                character.objectReferenceValue = SearchForNarrator();
                var emotion = el.FindPropertyRelative("emotion");
                emotion.stringValue = "Default";
                obj.ApplyModifiedProperties();
            }
            if (GUILayout.Button(new GUIContent()
            {
                text = "-"
            }, EditorStyles.miniButton))
            {
                if(list.arraySize > 0)
                {
                    if (highlightedIndex != -1)
                    {
                        for (int i = highlightedIndex; i < list.arraySize - 1; i++)
                        {
                            list.MoveArrayElement(i + 1, i);
                        }
                        highlightedIndex = -1;
                    }
                    list.arraySize--;
                }
            }
        }
        private void ListenForEvents(SerializedProperty list, Event e)
        {
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                for (int i = 0; i < dialoguesRect.Count; i++)
                {
                    if (CheckBoxCollision(e.mousePosition, dialoguesRect[i]))
                    {
                        GUI.FocusControl(null);
                        highlightedIndex = i;
                    }
                }
            }
            if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                highlightedIndex = -1;
            }
        }

        private bool CheckBoxCollision(Vector2 pos, Rect buttonRect)
        {
            if (pos.x > buttonRect.x && pos.y > buttonRect.y &&
                pos.x < buttonRect.x + buttonRect.width &&
                pos.y < buttonRect.y + buttonRect.height)
                return true;
            return false;
        }

        float boxRectWidth = -1;
        #region Custom List Element Editors
        void DrawHighlightedEditor(SerializedProperty list)
        {
            var commandElement = list.GetArrayElementAtIndex(highlightedIndex);
            var commandType = commandElement.FindPropertyRelative("type");
            CommandType type = (CommandType)commandType.enumValueIndex;
            switch (type)
            {
                case CommandType.SAY:
                    {
                        DrawHighlightedDialogueEditor_Say(list);
                        break;
                    }
                case CommandType.WAIT:
                    {
                        DrawHighlightedDialogueEditor_Wait(list);
                        break;
                    }
                case CommandType.SHOW:
                    {
                        DrawHighlightedDialogueEditor_Show(list);
                        break;
                    }
                case CommandType.MOVE:
                    {
                        DrawHighlightedDialogueEditor_Move(list);
                        break;
                    }
                case CommandType.SPRITE:
                    {
                        DrawHighlightedDialogueEditor_Sprite(list);
                        break;
                    }
            }

        }

        private void DrawHighlightedDialogueEditor_Show(SerializedProperty list)
        {
            //need character data dropdown, position, transition type and is show
            //if transition type is fade, time shows up

            var element = list.GetArrayElementAtIndex(highlightedIndex);
            //time property
            var timeProperty = element.FindPropertyRelative("Time");
            float timeValue = timeProperty.floatValue;
            //character property
            var characterProperty = element.FindPropertyRelative("Character");
            var character = new SerializedObject(element.FindPropertyRelative("Character").objectReferenceValue);
            List<string> characterNames = characters.Select(character => character.characterName).ToList();
            if(characterNames.Count > 1)
            {
                characterNames.Remove("Narrator");
            }
            var charOptions = characterNames.ToArray();
            var selectedCharacterName = character.FindProperty("characterName").stringValue;
            int selectedCharIndex = characterNames.IndexOf(selectedCharacterName);
            if (selectedCharIndex == -1)
                selectedCharIndex = 0;
            //emotion property
            var emotionProperty = element.FindPropertyRelative("emotion");
            var currentCharacterData = GetCharacterByName(selectedCharacterName);
            //get all emotions of a certain character
            string[] emotionOptions = currentCharacterData.emotions.Select(em => em.emotion).ToArray();
            //if the character has no emotions, create one for it
            if (emotionOptions.Length == 0)
                emotionOptions = new[] { "Default" };
            var emotionsList = emotionOptions.ToList();
            int selectedEmotionIndex = emotionsList.IndexOf(emotionProperty.stringValue);
            //if we didn't find the emotion inside the list, this means we're in the default branch
            if (selectedEmotionIndex == -1)
                selectedEmotionIndex = 0;
            //location property
            var posProperty = element.FindPropertyRelative("LocationTo");
            string[] posOptions = { "Left", "Centre", "Right" };
            int selectedPosIndex = posProperty.enumValueIndex;
            //transition property
            var transProperty = element.FindPropertyRelative("TransitionType");
            string[] transOptions = { "None", "Fade", "Punch" };
            int selectedTransIndex = transProperty.enumValueIndex;
            //isShow property
            var isShowProperty = element.FindPropertyRelative("IsShow");
            bool isShow = isShowProperty.boolValue;

            Rect boxRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);
            EditorGUILayout.LabelField(new GUIContent()
            {
                text = "Show",
                tooltip = "Show or hide a character or the scene background"
            }, EditorStyles.boldLabel);

            if (charOptions.Length == 1 && charOptions[0] == "Narrator")
            {
                EditorGUILayout.LabelField("There are no viable characters to be shown!\n" +
                    "Create a character using the 'Add Character' button on the Character List tab!", new[]
                    {
                        GUILayout.MinHeight(100)
                    });
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Character", GUILayout.Width(100));
            selectedCharIndex = EditorGUILayout.Popup(selectedCharIndex, charOptions);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Emotion", GUILayout.Width(100));
            selectedEmotionIndex = EditorGUILayout.Popup(selectedEmotionIndex, emotionOptions);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Position", GUILayout.Width(100));
            selectedPosIndex = EditorGUILayout.Popup(selectedPosIndex, posOptions);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Transition", GUILayout.Width(100));
            selectedTransIndex = EditorGUILayout.Popup(selectedTransIndex, transOptions);
            EditorGUILayout.EndHorizontal();
            if (transOptions[selectedTransIndex] == "Fade")
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Time", GUILayout.Width(100));
                timeValue = EditorGUILayout.FloatField(timeValue);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hide?", GUILayout.Width(100));
            isShow = EditorGUILayout.Toggle(isShow);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            timeProperty.floatValue = timeValue;
            if (boxRectWidth == -1 && boxRect.width != 0)
            {
                boxRectWidth = boxRect.width;
            }
            characterProperty.objectReferenceValue = GetCharacterByName(charOptions[selectedCharIndex]);
            emotionProperty.stringValue = emotionOptions[selectedEmotionIndex];
            timeProperty.floatValue = timeValue;
            posProperty.enumValueIndex = selectedPosIndex;
            transProperty.enumValueIndex = selectedTransIndex;
            isShowProperty.boolValue = isShow;

        }
        private void DrawHighlightedDialogueEditor_Wait(SerializedProperty list)
        {

            var timeProperty = list.GetArrayElementAtIndex(highlightedIndex).FindPropertyRelative("Time");
            float timeValue = timeProperty.floatValue;
            //needs time and that's it
            Rect boxRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);
            EditorGUILayout.LabelField(new GUIContent()
            {
                text = "Wait",
                tooltip = "Wait for the specified amount of seconds"
            }, EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time");
            timeValue = EditorGUILayout.FloatField(timeValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            timeProperty.floatValue = timeValue;
            if (boxRectWidth == -1 && boxRect.width != 0)
            {
                boxRectWidth = boxRect.width;
            }
            timeProperty.floatValue = timeValue;
        }
        private void DrawHighlightedDialogueEditor_Say(SerializedProperty list)
        {
            string text = "";
            string[] options = { };

            var dialogue = list.GetArrayElementAtIndex(highlightedIndex);
            var characterProperty = dialogue.FindPropertyRelative("Character");
            var character = new SerializedObject(dialogue.FindPropertyRelative("Character").objectReferenceValue);
            var dialogueText = dialogue.FindPropertyRelative("dialogueText");

            text = dialogueText.stringValue;
            List<string> characterNames = characters.Select(character => character.characterName).ToList();
            options = characterNames.ToArray();
            var selectedCharacterName = character.FindProperty("characterName").stringValue;
            int selectedIndex = characterNames.IndexOf(selectedCharacterName);

            var isShowProperty = dialogue.FindPropertyRelative("IsShow");
            bool isShow = isShowProperty.boolValue;

            var refreshProperty = dialogue.FindPropertyRelative("Refresh");
            bool refresh = refreshProperty.boolValue;

            var transProperty = dialogue.FindPropertyRelative("TransitionType");
            string[] transOptions = Enum.GetNames(typeof(TransitionTypes)).ToArray();
            int selectedTransIndex = transProperty.enumValueIndex;

            var timeProperty = dialogue.FindPropertyRelative("Time");
            float time = timeProperty.floatValue;

            Rect boxRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);
            EditorGUILayout.LabelField(new GUIContent()
            {
                text = "Say",
                tooltip = "Display a text box on screen with the specified text"
            }, EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Character", GUILayout.Width(100));
            selectedIndex = EditorGUILayout.Popup(selectedIndex, options);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Story text", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            EditorStyles.textArea.wordWrap = true;
            text = EditorGUILayout.TextArea(text, EditorStyles.textArea, new[]
            {
            GUILayout.MinWidth(20),
            GUILayout.MinHeight(200),
            GUILayout.ExpandHeight(true),
        });
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
            //if the selected character is not the narrator, 'refresh' is implied
            if(selectedCharacterName == "Narrator")
            {

                EditorGUILayout.PropertyField(refreshProperty, new GUIContent()
                {
                    text = "Refresh text box?",
                    tooltip = "If the box is ticked, the narrator text box is cleared prior to this message being shown"
                });
            }
            else
            {
                EditorGUILayout.PropertyField(refreshProperty, new GUIContent()
                {
                    text = "Auto-scroll text box?",
                    tooltip = "If the text would not fit inside the text box, it will not require player input to scroll.\n\n" +
                    " Useful for characters freaking out or talking very quickly"
                });
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(isShowProperty, new GUIContent()
            {
                text = "Hide text box?",
                tooltip = "If ticked, hides text box after the text is finished."
            });
            EditorGUILayout.EndHorizontal();
            GUI.enabled = isShow;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hide transition", GUILayout.Width(160));
            selectedTransIndex = EditorGUILayout.Popup(selectedTransIndex, transOptions);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (transOptions[selectedTransIndex] != "NONE")
            {
                if (transOptions[selectedTransIndex] == "FADE")
                    EditorGUILayout.PropertyField(timeProperty, new GUIContent()
                    {
                        text = "Fade time",
                        tooltip = "Over how many seconds does the textbox fade away."
                    });
                else
                    EditorGUILayout.PropertyField(timeProperty, new GUIContent()
                    {
                        text = "Punch strength",
                        tooltip = "How strong the camera shake should be. Recommended value: 10."
                    });
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
            if (boxRectWidth == -1 && boxRect.width != 0)
            {
                boxRectWidth = boxRect.width;
            }

            transProperty.enumValueIndex = selectedTransIndex;
            characterProperty.objectReferenceValue = GetCharacterByName(options[selectedIndex]);
            dialogueText.stringValue = text;
        }
        private void DrawHighlightedDialogueEditor_Move(SerializedProperty list)
        {

            var element = list.GetArrayElementAtIndex(highlightedIndex);
            //time property
            var timeProperty = element.FindPropertyRelative("Time");
            //character property
            var characterProperty = element.FindPropertyRelative("Character");
            var character = new SerializedObject(element.FindPropertyRelative("Character").objectReferenceValue);
            List<string> characterNames = characters.Select(character => character.characterName).ToList();
            if (characterNames.Count > 1)
            {
                characterNames.Remove("Narrator");
            }
            var charOptions = characterNames.ToArray();
            var selectedCharacterName = character.FindProperty("characterName").stringValue;
            int selectedCharIndex = characterNames.IndexOf(selectedCharacterName);
            if (selectedCharIndex == -1)
                selectedCharIndex = 0;
            //location property
            var posProperty = element.FindPropertyRelative("LocationTo");
            string[] posOptions = { "Left", "Centre", "Right" };
            int selectedPosIndex = posProperty.enumValueIndex;
            //Hide text box property
            var hideTextBoxProperty = element.FindPropertyRelative("IsShow");
            bool hideTextBox = hideTextBoxProperty.boolValue;

            Rect boxRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);
            EditorGUILayout.LabelField(new GUIContent()
            {
                text = "Move",
                tooltip = "Move a character sprite to another predetermined position"
            }, EditorStyles.boldLabel);

            if (charOptions.Length == 1 && charOptions[0] == "Narrator")
            {
                EditorGUILayout.LabelField("There are no viable characters to be moved!\n" +
                    "Create a character using the 'Add Character' button on the Character List tab!", new[]
                    {
                        GUILayout.MinHeight(100)
                    });
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Character", GUILayout.Width(100));
            selectedCharIndex = EditorGUILayout.Popup(selectedCharIndex, charOptions);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Position", GUILayout.Width(100));
            selectedPosIndex = EditorGUILayout.Popup(selectedPosIndex, posOptions);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(timeProperty);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(hideTextBoxProperty, new GUIContent()
            {
                text = "Hide text box?",
                tooltip = "Hides text box as the character is moving.\n " +
                "It will only show after another 'SAY' command."
            });
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (boxRectWidth == -1 && boxRect.width != 0)
            {
                boxRectWidth = boxRect.width;
            }
            characterProperty.objectReferenceValue = GetCharacterByName(charOptions[selectedCharIndex]);
            posProperty.enumValueIndex = selectedPosIndex;
        }
        private void DrawHighlightedDialogueEditor_Sprite(SerializedProperty list)
        {

            var element = list.GetArrayElementAtIndex(highlightedIndex);

            //character property
            var characterProperty = element.FindPropertyRelative("Character");
            var character = new SerializedObject(element.FindPropertyRelative("Character").objectReferenceValue);
            List<string> characterNames = characters.Select(character => character.characterName).ToList();
            if (characterNames.Count > 1)
            {
                characterNames.Remove("Narrator");
            }
            var charOptions = characterNames.ToArray();
            var selectedCharacterName = character.FindProperty("characterName").stringValue;
            int selectedCharIndex = characterNames.IndexOf(selectedCharacterName);
            if (selectedCharIndex == -1)
                selectedCharIndex = 0;
            //emotion property
            var emotionProperty = element.FindPropertyRelative("emotion");
            var currentCharacterData = GetCharacterByName(selectedCharacterName);
            //get all emotions of a certain character
            string[] emotionOptions = currentCharacterData.emotions.Select(em => em.emotion).ToArray();
            //if the character has no emotions, create one for it
            if (emotionOptions.Length == 0)
                emotionOptions = new[] { "Default" };
            var emotionsList = emotionOptions.ToList();
            int selectedEmotionIndex = emotionsList.IndexOf(emotionProperty.stringValue);
            //if we didn't find the emotion inside the list, this means we're in the default branch
            if (selectedEmotionIndex == -1)
                selectedEmotionIndex = 0;

            Rect boxRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);
            EditorGUILayout.LabelField(new GUIContent()
            {
                text = "Sprite",
                tooltip = "Change an active character's sprite to another from its emotion palette"
            }, EditorStyles.boldLabel);


            if (charOptions.Length == 1 && charOptions[0] == "Narrator")
            {
                EditorGUILayout.LabelField("There are no viable characters to change the sprite of!\n" +
                    "Create a character using the 'Add Character' button on the Character List tab!", new[]
                    {
                        GUILayout.MinHeight(100)
                    });
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Character", GUILayout.Width(100));
            selectedCharIndex = EditorGUILayout.Popup(selectedCharIndex, charOptions);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Emotion", GUILayout.Width(100));
            selectedEmotionIndex = EditorGUILayout.Popup(selectedEmotionIndex, emotionOptions);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            characterProperty.objectReferenceValue = GetCharacterByName(charOptions[selectedCharIndex]);
            emotionProperty.stringValue = emotionOptions[selectedEmotionIndex];
        }
        #endregion

        private CharacterData SearchForNarrator()
        {
            var assets = AssetDatabase.FindAssets("", new[] { "Assets/Scriptable Objects/Characters/" });
            List<CharacterData> characters = new List<CharacterData>();
            foreach (var asset in assets)
            {
                characters.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset), typeof(CharacterData)) as CharacterData);
            }
            return characters.FirstOrDefault(x => x.characterName == "Narrator");
        }

        private List<CharacterData> GetAllCharacters()
        {
            var assets = AssetDatabase.FindAssets("", new[] { "Assets/Scriptable Objects/Characters/" });
            List<CharacterData> characters = new List<CharacterData>();
            foreach (var asset in assets)
            {
                characters.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset), typeof(CharacterData)) as CharacterData);
            }
            return characters;
        }

        private void OpenCharacterByName(string name)
        {
            var assets = AssetDatabase.FindAssets("", new[] { "Assets/Scriptable Objects/Characters/" });
            foreach (var asset in assets)
            {
                var cd = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset), typeof(CharacterData));
                if (name == "<None>")
                {
                    if (cd.name == "")
                        AssetDatabase.OpenAsset(cd.GetInstanceID());
                }
                else if (name == cd.name)
                    AssetDatabase.OpenAsset(cd.GetInstanceID());

            }
        }
        private CharacterData GetCharacterByName(string name)
        {

            var assets = AssetDatabase.FindAssets("", new[] { "Assets/Scriptable Objects/Characters/" });
            List<CharacterData> characters = new List<CharacterData>();
            foreach (var asset in assets)
            {
                characters.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset), typeof(CharacterData)) as CharacterData);
            }
            return characters.FirstOrDefault(x => x.characterName == name);
        }
    }
}
