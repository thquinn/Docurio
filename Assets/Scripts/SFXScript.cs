using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXScript : MonoBehaviour
{
    public static SFXScript instance;
    public AudioSource hover;

    void Start()
    {
        instance = this;
    }

    public void Hover() {
        hover.PlayOneShot(hover.clip);
    }
}
