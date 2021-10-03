using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperIdleAnimScript : MonoBehaviour
{
    public Transform[] rings;
    public Vector3[] axes, axisShifts;

    void Start() {
        axes = new Vector3[rings.Length];
        axisShifts = new Vector3[rings.Length];
        for (int i = 0; i < axes.Length; i++) {
            axes[i] = Random.insideUnitSphere.normalized;
            axisShifts[i] = Random.insideUnitSphere.normalized;
        }
    }
    void Update() {
        for (int i = 0; i < rings.Length; i++) {
            rings[i].Rotate(axes[i], 1);
        }
        for (int i = 0; i < axes.Length; i++) {
            axes[i] = (axes[i] + axisShifts[i] * .1f).normalized;
        }
    }
}
