using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PusherIdleAnimScript : MonoBehaviour {
    static Vector3[] POSITIONS = new Vector3[] { new Vector3(.5f, .5f, -.5f), new Vector3(.5f, 1.5f, -.5f), new Vector3(.5f, 1.5f, .5f), new Vector3(-.5f, 1.5f, .5f), new Vector3(-.5f, 1.5f, -.5f), new Vector3(-.5f, .5f, -.5f), new Vector3(-.5f, .5f, .5f), new Vector3(.5f, .5f, .5f) };
    static float SPEED = .1f;
    static float CLOSE_ENOUGH = .01f;

    public Transform[] cubes;

    public int indexOffset = 0;
    public float diff = 1;

    void Update() {
        diff = Mathf.Lerp(diff, 0, SPEED);
        float t = SPEED;
        if (diff <= CLOSE_ENOUGH) {
            t = 1;
            diff = 1;
        }
        for (int i = 0; i < cubes.Length; i++) {
            int index = (i * 2 + indexOffset) % POSITIONS.Length;
            cubes[i].localPosition = Vector3.Lerp(cubes[i].localPosition, POSITIONS[index], t);
        }
        if (t == 1) {
            indexOffset++;
            if (indexOffset % 4 == 0) {
                float selector = Random.value;
                if (selector < .25f) {
                    return;
                } else if (selector < .5f) {
                    transform.Rotate(0, 180, 0);
                } else if (selector < .75f) {
                    transform.Rotate(0, Random.value < .5 ? -90 : 90, 0);
                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                } else {
                    transform.Rotate(0, Random.value < .5 ? -90 : 90, 0);
                    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, -transform.localScale.z);
                }
            }
        }
    }
}
