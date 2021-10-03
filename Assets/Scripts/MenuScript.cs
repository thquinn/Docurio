using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    public TextAsset[] levelTexts;
    public GameObject levelRowPrefab, gameBoardPrefab;
    public GameObject menuScroll;
    public CameraScript cameraScript;

    GameBoardScript gameBoardScript;

    void Start() {
        Application.targetFrameRate = 60;
        Transform footerTransform = menuScroll.transform.GetChild(menuScroll.transform.childCount - 1);
        for (int i = 0; i < levelTexts.Length; i++) {
            LevelRowScript levelRowScript = Instantiate(levelRowPrefab, menuScroll.transform).GetComponent<LevelRowScript>();
            string name = levelTexts[i].name;
            name = name.Substring(name.IndexOf(' '));
            levelRowScript.Set(i, name, levelTexts[i].text);
        }
        footerTransform.SetAsLastSibling();
        ButtonScript.OnSelectLevel += OnSelectLevel;
    }

    void OnSelectLevel(LevelInfo levelInfo) {
        gameBoardScript = Instantiate(gameBoardPrefab).GetComponent<GameBoardScript>();
        gameBoardScript.Init(levelInfo);
        menuScroll.SetActive(false);
        cameraScript.mode = CameraMode.Game;
    }
}