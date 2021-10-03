using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScribbleScript : MonoBehaviour
{
    public Image image;
    public Sprite otherScribble;
    Sprite firstScribble;
    int frame;

    void Start() {
        firstScribble = image.sprite;
    }
    void Update()
    {
        if (++frame == 20) {
            image.sprite = otherScribble;
        }
        if (frame == 40) {
            image.sprite = firstScribble;
            frame = 0;
        }
    }
}
