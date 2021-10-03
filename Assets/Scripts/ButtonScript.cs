using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    static Vector3 HOVER_DOWN = new Vector3(0, -6, 0);
    static Vector3 HOVER_UP = new Vector3(0, 10, 0);
    public GameObject shadow, button;
    public TextMeshProUGUI difficultyText;
    public CanvasGroup difficultyGroup;
    bool hovered;
    #pragma warning disable 0649
    public LevelInfo levelInfo;
    #pragma warning restore 0649

    // Update is called once per frame
    void Update()
    {
        shadow.transform.localPosition = Vector3.Lerp(shadow.transform.localPosition, hovered ? HOVER_DOWN : Vector3.zero, .1f);
        button.transform.localPosition = Vector3.Lerp(button.transform.localPosition, hovered ? HOVER_UP : Vector3.zero, .1f);
        if (hovered) {
            difficultyGroup.alpha = Mathf.Clamp01(difficultyGroup.alpha + .08f);
        } else {
            difficultyGroup.alpha = Mathf.Clamp01(difficultyGroup.alpha - .02f);
        }
    }

    public void SetLevelInfo(LevelInfo levelInfo) {
        this.levelInfo = levelInfo;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        hovered = true;
        difficultyText.text = gameObject.name;
    }

    public void OnPointerExit(PointerEventData eventData) {
        hovered = false;
    }

    public delegate void SelectLevelAction(LevelInfo levelInfo);
    public static event SelectLevelAction OnSelectLevel;
    public void OnPointerClick(PointerEventData eventData) {
        OnSelectLevel(levelInfo);
    }
}
