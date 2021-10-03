using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsPopupScript : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    int frames = 0;

    void Start()
    {
        canvasGroup.alpha = 0;
    }

    void Update()
    {
        frames++;
        if (frames < 600) {
            if (frames > 30) {
                canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha + .01f);
            }
        } else {
            canvasGroup.alpha -= .01f;
            if (canvasGroup.alpha <= 0) {
                Destroy(gameObject);
            }
        }
    }
}
