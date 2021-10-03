using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectTileScript : MonoBehaviour
{
    public static float FADE_RATE = .033f;

    public SpriteRenderer spriteRenderer;

    Vector3 targetPos;

    void Start() {
        targetPos = spriteRenderer.transform.localPosition;
        spriteRenderer.transform.localPosition = new Vector3(0, .33f, 0);
        Color c = spriteRenderer.color;
        c.a = 0;
        spriteRenderer.color = c;
    }

    void Update() {
        spriteRenderer.transform.localPosition = Vector3.Lerp(spriteRenderer.transform.localPosition, targetPos, .4f);
        Color c = spriteRenderer.color;
        c.a += FADE_RATE;
        spriteRenderer.color = c;
        if (c.a >= 1) {
            Destroy(this);
        }
    }
}
