using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingScript : MonoBehaviour
{
    void Start() {
        GameObject halfcyl = transform.GetChild(0).gameObject;
        for (int i = 1; i < 5; i++) {
            GameObject copy = Instantiate(halfcyl, transform);
            copy.transform.Translate(0, halfcyl.transform.localScale.y * i * 2, 0);
        }
    }

    void Update() {
        float theta = .1f;
        foreach (Transform childTransform in transform) {
            childTransform.Rotate(0, theta, 0);
            theta += .05f;
        }
    }
}
