using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectBlockScript : MonoBehaviour
{
    static float ICON_SCALE = .55f;
    public GameObject anchor, icon;
    public SpriteRenderer squareRenderer, iconRenderer;

    void Start() {
        anchor.transform.localScale = new Vector3(1, GameBoardScript.entityHeight, 1);
        icon.transform.localScale = new Vector3(ICON_SCALE, ICON_SCALE / GameBoardScript.entityHeight, ICON_SCALE);
        Color c = squareRenderer.color;
        c.a = 0;
        squareRenderer.color = c;
        iconRenderer.color = c;
    }

    void Update() {
        Color c = squareRenderer.color;
        c.a += SelectTileScript.FADE_RATE;
        squareRenderer.color = c;
        iconRenderer.color = c;
        if (c.a >= 1) {
            Destroy(this);
        }
    }
}
