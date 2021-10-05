using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Vector3 lookAt;
    public float distance;
    public float sensitivity;
    public float scrollSensitivity;
    float horizontalAngle = Mathf.PI * 2/3;
    float verticalAngle = Mathf.PI / 6;

    public CameraMode mode = CameraMode.Menu, lastMode = CameraMode.Menu;
    Tuple<float, float> target;

    private void Update() {
        // Level transition.
        if (lastMode == CameraMode.Menu && mode == CameraMode.Game) {
            target = new Tuple<float, float>(Mathf.PI * 2 / 3, Mathf.PI / 6);
        }
        if (target != null) {
            horizontalAngle = Mathf.Lerp(horizontalAngle, target.Item1, .033f);
            verticalAngle = Mathf.Lerp(verticalAngle, target.Item2, .033f);
            if (Mathf.Max(Mathf.Abs(horizontalAngle - target.Item1), Mathf.Abs(verticalAngle - target.Item2)) < .1f) {
                target = null;
            }
        }
        lastMode = mode;

        // Input.
        if (mode == CameraMode.Game && target == null) {
            distance *= Mathf.Pow(scrollSensitivity, Input.mouseScrollDelta.y);
            distance = Mathf.Clamp(distance, 4, 10);
            if (Input.GetMouseButton(1)) {
                horizontalAngle -= Input.GetAxis("Mouse X") * sensitivity;
                verticalAngle -= Input.GetAxis("Mouse Y") * sensitivity;
                verticalAngle = Mathf.Clamp(verticalAngle, Mathf.PI * .1f, Mathf.PI * .49f);
            }
        }

        // Set position.
        float xzDistance = distance * Mathf.Cos(verticalAngle);
        float x = Mathf.Cos(horizontalAngle) * xzDistance;
        float y = Mathf.Sin(verticalAngle) * distance;
        float z = Mathf.Sin(horizontalAngle) * xzDistance;
        transform.localPosition = new Vector3(x, y, z);
        transform.LookAt(lookAt);
    }
}

public enum CameraMode {
    Menu, Game
}
