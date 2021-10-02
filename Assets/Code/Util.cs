using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code {
    class Util {
        public static Int3 FindIndex3(object[,,] arr, object o) {
            for (int x = 0; x < arr.GetLength(0); x++) {
                for (int y = 0; y < arr.GetLength(1); y++) {
                    for (int z = 0; z < arr.GetLength(2); z++) {
                        if (arr[x, y, z] == o) {
                            return new Int3(x, y, z);
                        }
                    }
                }
            }
            return Int3.None;
        }

        static Camera mainCamera;
        public static Collider GetMouseCollider(LayerMask layerMask) {
            if (mainCamera == null) mainCamera = Camera.main;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) {
                return null;
            }
            return hit.collider;
        }
    }

    public struct Int2 {
        public int x, y;

        public Int2(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public override string ToString() {
            return string.Format("({0}, {1})", x, y);
        }
    }

    public struct Int3 {
        public static Int3 None = new Int3(-1, -1, -1);
        public int x, y, z;

        public Int3(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString() {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }
    }
}
