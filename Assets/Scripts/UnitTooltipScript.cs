using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitTooltipScript : MonoBehaviour
{
    static int STAY_FRAMES = 30;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI tmp;
    GameBoardScript gameBoardScript;
    LayerMask layerMask;
    Collider lastCollider;
    int stayFrames;

    void Start() {
        canvasGroup.alpha = 0;
    }
    public void Set(GameBoardScript gameBoardScript, LayerMask layerMask) {
        this.gameBoardScript = gameBoardScript;
        this.layerMask = layerMask;
    }

    void Update() {
        Collider collider = Util.GetMouseCollider(layerMask);
        if (collider != lastCollider) {
            lastCollider = collider;
            stayFrames = 0;
            canvasGroup.alpha = 0;
        } else if (collider != null) {
            stayFrames++;
            if (stayFrames == STAY_FRAMES) {
                Int3 from = Util.FindIndex3(gameBoardScript.entityScripts, collider.GetComponent<EntityScript>());
                if (from == Int3.None) {
                    lastCollider = null;
                    stayFrames = 0;
                    canvasGroup.alpha = 0;
                    return;
                }
                SetText(gameBoardScript.state.Get(from));
            }
            if (stayFrames >= STAY_FRAMES) {
                canvasGroup.alpha += .1f;
            }
        }
    }

    void SetText(DocurioEntity unit) {
        if ((unit & DocurioEntity.King) != 0) {
            tmp.text = "<size=50>King</size>\nMoves one square in any direction. Can't climb. If captured, you lose.";
        } else if ((unit & DocurioEntity.Pusher) != 0) {
            tmp.text = "<size=50>Pusher</size>\nMoves any distance in a cardinal direction. Can push blocks.";
        } else if ((unit & DocurioEntity.Sniper) != 0) {
            tmp.text = "<size=50>Sniper</size>\nTeleports up to two squares without capturing. Can shoot distant enemies in a straight line.";
        } else if ((unit & DocurioEntity.Bystander) != 0) {
            tmp.text = "<size=50>Peon</size>\nMoves one square in any direction. Can't capture.";
        }
    }
}
