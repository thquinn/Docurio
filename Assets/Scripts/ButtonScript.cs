using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    public Image icon, sideCard, topCard;
    public Sprite checkSprite;

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

    public void SetComplete() {
        icon.sprite = checkSprite;
        icon.color = new Color(0.7882354f, 0.8039216f, 0.7843138f, 1);
        sideCard.color = new Color(0.682353f, 0.7215686f, 0.6588235f, 1);
        topCard.color = new Color(0.6470588f, 0.6901961f, 0.6235294f, 1);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        hovered = true;
        difficultyText.text = gameObject.name;
        SFXScript.instance.Hover();
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
