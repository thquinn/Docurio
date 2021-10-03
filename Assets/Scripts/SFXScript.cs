using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXScript : MonoBehaviour
{
    public static SFXScript instance;
    public AudioSource hover, push, run;

    void Start()
    {
        instance = this;
    }

    public void Hover() {
        hover.PlayOneShot(hover.clip);
    }
    public void Push() {
        push.PlayOneShot(push.clip);
    }
    public void Run() {
        run.PlayOneShot(run.clip);
    }
}
