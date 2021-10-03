using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiIndicatorScript : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public GameObject spinner;
    int waitFrames = 120;
    bool destroying;

    void Start() {
        canvasGroup.alpha = 0;
    }

    void Update() {
        if (waitFrames-- > 0) {
            return;
        }
        spinner.transform.Rotate(0, 0, -3.1f);
        if (this.destroying) {
            canvasGroup.alpha -= .1f;
            if (canvasGroup.alpha <= 0) {
                Destroy(gameObject);
            }
        } else {
            canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha + .1f);
        }
    }

    public void Destroy() {
        destroying = true;
    }
}
