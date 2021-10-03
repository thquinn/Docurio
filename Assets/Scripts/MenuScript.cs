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

    GameBoardScript gameBoardScript;

    void Start() {
        Application.targetFrameRate = 60;
        Transform footerTransform = menuScroll.transform.GetChild(menuScroll.transform.childCount - 1);
        TextAsset[] levelTexts = Resources.LoadAll<TextAsset>("Levels");
        for (int i = 0; i < levelTexts.Length; i++) {
            LevelRowScript levelRowScript = Instantiate(levelRowPrefab, menuScroll.transform).GetComponent<LevelRowScript>();
            string name = levelTexts[i].name;
            name = name.Substring(name.IndexOf(' '));
            levelRowScript.Set(i, name, levelTexts[i].text);
        }
        footerTransform.SetAsLastSibling();
        ButtonScript.OnSelectLevel += OnSelectLevel;
    }

    void Update() {
        if (gameBoardScript != null) {
            int winner = gameBoardScript.Winner();
            if (winner != -1 || Input.GetKeyDown(KeyCode.Escape)) {
                Destroy(gameBoardScript.gameObject);
                gameBoardScript = null;
                cameraScript.mode = CameraMode.Menu;
            }
        }
        if (!scrollRect.enabled) {
            if (gameBoardScript != null) {
                menuScroll.transform.parent.localPosition = Vector3.Lerp(menuScroll.transform.parent.localPosition, new Vector3(0, -1200, 0), .033f);
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
        gameBoardScript = Instantiate(gameBoardPrefab).GetComponent<GameBoardScript>();
        gameBoardScript.Init(levelInfo);
        scrollRect.enabled = false;
        cameraScript.mode = CameraMode.Game;
        if (levelInfo.index == 0) {
            Instantiate(controlsPopupPrefab, canvasTransform);
        }
    }
}