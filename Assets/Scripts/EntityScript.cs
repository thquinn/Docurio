using Assets.Code;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityScript : MonoBehaviour
{
    public Material blackMaterial;
    public MeshFilter meshFilter;
    public GameObject prefabPusherAnim;

    Queue<UnitAnimation> animations = new Queue<UnitAnimation>();

    public void BecomePusher() {
        Destroy(meshFilter.gameObject);
        Instantiate(prefabPusherAnim, transform);
    }
    public void BecomeBlack() {
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>()) {
            mr.material = blackMaterial;
        }
    }

    void Update() {
        if (animations.Count == 0) {
            return;
        }
        UnitAnimation animation = animations.Peek();
        transform.localPosition = animation.Tick();
        if (animation.IsDone()) {
            animations.Dequeue();
        }
    }

    public bool IsAnimating() {
        return animations.Count > 0;
    }

    public void AnimateLinearMove(DocurioState state, DocurioMove move) {
        int x = move.from.x;
        int y = move.from.y;
        int z = state.GroundZ(x, y);
        int dx = Util.SignFixed(move.to.x - x);
        int dy = Util.SignFixed(move.to.y - y);
        Int3 runStart = move.from;
        while (x != move.to.x || y != move.to.y) {
            Int3 prev = new Int3(x, y, z);
            x += dx;
            y += dy;
            int nextZ = state.GroundZ(x, y);
            Int3 next = new Int3(x, y, nextZ);
            if (nextZ != z) {
                if (prev != runStart) {
                    animations.Enqueue(new UnitRunAnimation(runStart, prev));
                }
                if (nextZ < z) {
                    animations.Enqueue(new UnitDropAnimation(prev, next));
                } else {
                    animations.Enqueue(new UnitJumpAnimation(prev, next));
                }
                runStart = next;
            }
            z = nextZ;
        }
        Int3 end = new Int3(x, y, z);
        if (end != runStart) {
            animations.Enqueue(new UnitRunAnimation(runStart, end));
        }
    }
    public void AnimatePushes(Int3 loc, Dictionary<EntityScript, Tuple<Int3, Int3>> slides) {
        animations.Enqueue(new UnitPushAnimation(loc, slides));
    }
    public void AnimateSlideAndDrop(Tuple<Int3, Int3> slide) {
        Int3 beforeDrop = slide.Item2;
        beforeDrop.z = slide.Item1.z;
        animations.Enqueue(new UnitSlideAnimation(slide.Item1, beforeDrop));
        if (slide.Item1.z != slide.Item2.z) {
            animations.Enqueue(new UnitGravityAnimation(beforeDrop, slide.Item2));
        }
    }
}

abstract class UnitAnimation {
    protected int frame, totalFrames;
    protected Vector3 from, to;

    public UnitAnimation(Int3 from, Int3 to) {
        frame = 0;
        this.from = new Vector3(from.x, from.z * GameBoardScript.entityHeight, from.y);
        this.to = new Vector3(to.x, to.z * GameBoardScript.entityHeight, to.y);
    }

    public abstract Vector3 Tick();
    public bool IsDone() {
        return frame >= totalFrames;
    }
}

class UnitRunAnimation : UnitAnimation {
    static float RUN_SPEED = .066f;

    public UnitRunAnimation(Int3 from, Int3 to) : base(from, to) {
        int dx = to.x - from.x;
        int dy = to.y - from.y;
        float distance = Mathf.Sqrt(dx * dx + dy * dy);
        distance = Mathf.Sqrt(distance); // EaseInOutQuad causes longer distances to take proportionately longer
        totalFrames = Mathf.RoundToInt(distance / RUN_SPEED);
    }

    public override Vector3 Tick() {
        frame++;
        float t = frame / (float)totalFrames;
        t = EasingFunction.EaseInOutQuad(0, 1, t);
        return Vector3.Lerp(from, to, t);
    }
}

class UnitDropAnimation : UnitAnimation {
    static float FALL_SPEED = .066f;

    public UnitDropAnimation(Int3 from, Int3 to) : base(from, to) {
        int distance = from.z - to.z;
        totalFrames = Mathf.RoundToInt(Mathf.Sqrt(distance) / FALL_SPEED);
    }

    public override Vector3 Tick() {
        frame++;
        float t = frame / (float)totalFrames;
        float x = Mathf.Lerp(from.x, to.x, t);
        float y = EasingFunction.EaseInBack(from.y, to.y, t);
        float z = Mathf.Lerp(from.z, to.z, t);
        return new Vector3(x, y, z);
    }
}

class UnitJumpAnimation : UnitAnimation {
    static float JUMP_SPEED = .066f;

    public UnitJumpAnimation(Int3 from, Int3 to) : base(from, to) {
        int distance = to.z - from.z;
        totalFrames = Mathf.RoundToInt(Mathf.Sqrt(distance) / JUMP_SPEED);
    }

    public override Vector3 Tick() {
        frame++;
        float t = frame / (float)totalFrames;
        float x = Mathf.Lerp(from.x, to.x, t);
        float y = EasingFunction.EaseOutBack(from.y, to.y, t);
        float z = Mathf.Lerp(from.z, to.z, t);
        return new Vector3(x, y, z);
    }
}

class UnitPushAnimation : UnitAnimation {
    Dictionary<EntityScript, Tuple<Int3, Int3>> slides;

    public UnitPushAnimation(Int3 loc, Dictionary<EntityScript, Tuple<Int3, Int3>> slides) : base(loc, loc) {
        totalFrames = 0;
        this.slides = slides;
    }

    public override Vector3 Tick() {
        foreach (var kvp in slides) {
            kvp.Key.AnimateSlideAndDrop(kvp.Value);
        }
        return to;
    }
}

class UnitSlideAnimation : UnitAnimation {
    public UnitSlideAnimation(Int3 from, Int3 to) : base(from, to) {
        int dx = to.x - from.x;
        int dy = to.y - from.y;
        int distance = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
        totalFrames = Mathf.RoundToInt(15 * Mathf.Sqrt(distance));
    }

    public override Vector3 Tick() {
        frame++;
        float t = frame / (float)totalFrames;
        t = EasingFunction.EaseOutQuad(0, 1, t);
        return Vector3.Lerp(from, to, t);
    }
}

class UnitGravityAnimation : UnitAnimation {
    static Vector3 GRAVITY = new Vector3(0, -.01f, 0);
    Vector3 speed;

    public UnitGravityAnimation(Int3 from, Int3 to) : base(from, to) {
        totalFrames = 999;
        speed = Vector3.zero;
    }

    public override Vector3 Tick() {
        speed += GRAVITY;
        from += speed;
        if (from.y <= to.y) {
            from.y = to.y;
            totalFrames = 0;
        }
        return from;
    }
}