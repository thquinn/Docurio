using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public GameObject levelRowPrefab, gameBoardPrefab, controlsPopupPrefab;
    public Transform canvasTransform;
    public GameObject menuScroll;
    public ScrollRect scrollRect;
    public CameraScript cameraScript;
    bool[,] beatLevels;
    ButtonScript[,] buttonScripts;
    LevelInfo selectedLevel;

    GameBoardScript gameBoardScript;

    void Start() {
        Application.targetFrameRate = 60;
        Transform footerTransform = menuScroll.transform.GetChild(menuScroll.transform.childCount - 1);
        TextAsset[] levelTexts = Resources.LoadAll<TextAsset>("Levels");
        beatLevels = new bool[levelTexts.Length, 3];
        buttonScripts = new ButtonScript[levelTexts.Length, 3];
        int levelNum = 0;
        for (int i = 0; i < levelTexts.Length; i++) {
            LevelRowScript levelRowScript = Instantiate(levelRowPrefab, menuScroll.transform).GetComponent<LevelRowScript>();
            buttonScripts[levelNum, 0] = levelRowScript.buttons[0];
            buttonScripts[levelNum, 1] = levelRowScript.buttons[1];
            buttonScripts[levelNum, 2] = levelRowScript.buttons[2];
            levelNum++;
            int spaceIndex = levelTexts[i].name.IndexOf(' ');
            string name = levelTexts[i].name.Substring(spaceIndex + 1);
            if (levelTexts[i].name[spaceIndex - 1] == 'a') {
                levelRowScript.Set(levelNum, name, levelTexts[i].text, levelTexts[i + 1].text, levelTexts[i + 2].text);
                i += 2;
            } else {
                levelRowScript.Set(levelNum, name, levelTexts[i].text, levelTexts[i].text, levelTexts[i].text);
            }
        }
        footerTransform.SetAsLastSibling();
        ButtonScript.OnSelectLevel += OnSelectLevel;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) && selectedLevel == null && Application.platform != RuntimePlatform.WebGLPlayer && !Application.isEditor) {
            Application.Quit();
        }
        if (gameBoardScript != null) {
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, 1, .1f);
            int winner = gameBoardScript.Winner();
            if (winner != -1 || Input.GetKeyDown(KeyCode.Escape)) {
                if (winner == 0) {
                    int difficultyIndex = selectedLevel.difficulty == LevelDifficulty.Easy ? 0 : (selectedLevel.difficulty == LevelDifficulty.Standard ? 1 : 2);
                    while (difficultyIndex >= 0) {
                        beatLevels[selectedLevel.index, difficultyIndex] = true;
                        buttonScripts[selectedLevel.index, difficultyIndex].SetComplete();
                        difficultyIndex--;
                    }
                }
                // TODO: this can be moved to the game board script when it has a Destroy function
                if (gameBoardScript.aiIndicator != null) {
                    gameBoardScript.aiIndicator.Destroy();
                }
                Destroy(gameBoardScript.gameObject);
                gameBoardScript = null;
                cameraScript.mode = CameraMode.Menu;
                if (AI.thread != null) {
                    AI.thread.Abort();
                }
                AI.status = AIStatus.Ready;
                selectedLevel = null;
            }
        }
        if (!scrollRect.enabled) {
            if (gameBoardScript != null) {
                menuScroll.transform.parent.localPosition = Vector3.Lerp(menuScroll.transform.parent.localPosition, new Vector3(0, -2000, 0), .033f);
            } else {
                menuScroll.transform.parent.localPosition = Vector3.Lerp(menuScroll.transform.parent.localPosition, Vector3.zero, .1f);
                if (menuScroll.transform.parent.localPosition.sqrMagnitude < 5) {
                    menuScroll.transform.parent.localPosition = Vector3.zero;
                    scrollRect.enabled = true;
                }
            }
        }
    }

    void OnSelectLevel(LevelInfo levelInfo) {
        if (gameBoardScript != null) {
            return;
        }
        selectedLevel = levelInfo;
        gameBoardScript = Instantiate(gameBoardPrefab).GetComponent<GameBoardScript>();
        gameBoardScript.Init(levelInfo);
        scrollRect.enabled = false;
        cameraScript.mode = CameraMode.Game;
        if (levelInfo.index == 0) {
            Instantiate(controlsPopupPrefab, canvasTransform);
        }
    }
}