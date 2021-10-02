using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code {
    class Util {
        public static int SignFixed(int n) {
            if (n == 0) {
                return 0;
            }
            return n < 0 ? -1 : 1;
        }

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
        public static Int2 None = new Int2(-1, -1);
        public int x, y;

        public Int2(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public bool IsZero() {
            return x == 0 && y == 0;
        }

        public override string ToString() {
            return string.Format("({0}, {1})", x, y);
        }

        public override int GetHashCode() {
            return x ^ y;
        }
        public override bool Equals(object obj) {
            if (!(obj is Int2)) {
                return false;
            }
            Int2 other = (Int2)obj;
            return x == other.x && y == other.y;
        }
        public static bool operator ==(Int2 a, Int2 b) {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator !=(Int2 a, Int2 b) => !(a == b);
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

        public override int GetHashCode() {
            return x ^ y ^ z;
        }
        public override bool Equals(object obj) {
            if (!(obj is Int3)) {
                return false;
            }
            Int3 other = (Int3) obj;
            return x == other.x && y == other.y && z == other.z;
        }
        public static bool operator ==(Int3 a, Int3 b) {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }
        public static bool operator !=(Int3 a, Int3 b) => !(a == b);
    }
}
