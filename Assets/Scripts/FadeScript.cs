using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScript : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    void Start() {
        if (!Application.isEditor) {
            canvasGroup.alpha = 1;
        }
    }
    void Update()
    {
        canvasGroup.alpha -= .01f;
        if (canvasGroup.alpha <= 0) {
            Destroy(gameObject);
        }
    }
}
