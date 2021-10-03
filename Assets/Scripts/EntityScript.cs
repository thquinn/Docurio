using Assets.Code;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityScript : MonoBehaviour
{
    public Material blackMaterial;
    public MeshFilter meshFilter;
    public Mesh meshBystander;
    public GameObject prefabPusherAnim, prefabSniperAnim;
    Material sniperMaterial;

    Queue<UnitAnimation> animations = new Queue<UnitAnimation>();

    public void BecomePusher() {
        Destroy(meshFilter.gameObject);
        Instantiate(prefabPusherAnim, transform);
    }
    public void BecomeSniper() {
        Destroy(meshFilter.gameObject);
        Instantiate(prefabSniperAnim, transform);
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        sniperMaterial = new Material(renderers[0].material);
        foreach (MeshRenderer renderer in renderers) {
            renderer.material = sniperMaterial;
        }
    }
    public void BecomeBystander() {
        meshFilter.mesh = meshBystander;
        meshFilter.transform.localScale = new Vector3(.15f, .15f, .15f);
    }
    public void BecomeBlack() {
        if (sniperMaterial == null) {
            foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>()) {
                mr.material = blackMaterial;
            }
        } else {
            sniperMaterial = new Material(blackMaterial);
            foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>()) {
                mr.material = sniperMaterial;
            }
        }
    }

    void Update() {
        if (animations.Count == 0) {
            return;
        }
        UnitAnimation animation = animations.Peek();
        transform.localPosition = animation.Tick();
        if (sniperMaterial != null) {
            float opacity = animation.Opacity();
            Color c = sniperMaterial.color;
            if (c.a == 1 && opacity < 1) {
                sniperMaterial.ToFadeMode();
            } else if (c.a < 1 && opacity == 1) {
                sniperMaterial.ToOpaqueMode();
            }
            c.a = opacity;
            sniperMaterial.color = c;
        }
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
    public void AnimateTeleport(DocurioMove move) {
        animations.Enqueue(new UnitTeleportAnimation(move.from, move.to));
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
    public virtual float Opacity() {
        return 1;
    }
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
        if (frame == 0) {
            SFXScript.instance.Run();
        }
        frame++;
        float t = frame / (float)totalFrames;
        t = EasingFunction.EaseInOutQuad(0, 1, t);
        return Vector3.Lerp(from, to, t);
    }
}

class UnitDropAnimation : UnitAnimation {
    static float FALL_SPEED = .066f;
    bool sfx;

    public UnitDropAnimation(Int3 from, Int3 to) : base(from, to) {
        int distance = from.z - to.z;
        totalFrames = Mathf.RoundToInt(Mathf.Sqrt(distance) / FALL_SPEED);
    }

    public override Vector3 Tick() {
        frame++;
        float t = frame / (float)totalFrames;
        if (t > .2f && !sfx) {
            SFXScript.instance.Land();
            sfx = true;
        }
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
        if (frame == 0) {
            SFXScript.instance.Jump();
        }
        frame++;
        float t = frame / (float)totalFrames;
        float x = Mathf.Lerp(from.x, to.x, t);
        float y = EasingFunction.EaseOutBack(from.y, to.y, t);
        float z = Mathf.Lerp(from.z, to.z, t);
        return new Vector3(x, y, z);
    }
}

class UnitTeleportAnimation : UnitAnimation {
    float t;
    public UnitTeleportAnimation(Int3 from, Int3 to) : base(from, to) {
        totalFrames = 60;
    }

    public override Vector3 Tick() {
        frame++;
        t = (float)frame / totalFrames;
        if (t < .5) {
            t *= 2;
            return from + new Vector3(0, EasingFunction.EaseInQuad(0, 1, t), 0);
        } else {
            t = 1 - (t - .5f) * 2;
            return to + new Vector3(0, EasingFunction.EaseInQuad(0, 1, t), 0);
        }
    }
    public override float Opacity() {
        return 1 - t;
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
        SFXScript.instance.Push();
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