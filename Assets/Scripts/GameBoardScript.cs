using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardScript : MonoBehaviour
{
    public GameObject tilePrefab, blockPrefab, unitPrefab, selectTilePrefab, selectBlockPrefab;
    public LayerMask layerMaskSwitch, layerMaskMove;
    public static float entityHeight;

    LevelInfo levelInfo;
    DocurioState state;
    GameObject[,] tileObjects;
    EntityScript[,,] entityScripts;
    // Unit and move selection.
    bool[] aiControl;
    Dictionary<Collider, DocurioMove> selectMoveObjects = new Dictionary<Collider, DocurioMove>();

    public void Init(LevelInfo levelInfo) {
        this.levelInfo = levelInfo;
        state = new DocurioState(levelInfo.layout);
        transform.localPosition = new Vector3((state.xSize - 1) / -2f, 0, (state.ySize - 1) / -2f);
        tileObjects = new GameObject[state.xSize, state.ySize];
        entityScripts = new EntityScript[state.xSize, state.ySize, state.zSize];
        entityHeight = blockPrefab.transform.localScale.y;
        for (int x = 0; x < state.xSize; x++) {
            for (int y = 0; y < state.ySize; y++) {
                GameObject tile = Instantiate(tilePrefab, transform);
                tile.transform.localPosition = new Vector3(x, 0, y);
                tileObjects[x, y] = tile;
                for (int z = 0; z < state.zSize; z++) {
                    if (state.Is(x, y, z, DocurioEntity.Empty)) {
                        break;
                    }
                    GameObject entity;
                    if (state.board[x, y, z] == DocurioEntity.Block) {
                        entity = Instantiate(blockPrefab, transform);
                    } else if (state.Is(x, y, z, DocurioEntity.King)) {
                        entity = Instantiate(unitPrefab, transform);
                    } else if (state.Is(x, y, z, DocurioEntity.Pusher)) {
                        entity = Instantiate(unitPrefab, transform);
                        entity.GetComponent<EntityScript>().BecomePusher();
                    } else if (state.Is(x, y, z, DocurioEntity.Sniper)) {
                        entity = Instantiate(unitPrefab, transform);
                        entity.GetComponent<EntityScript>().BecomeSniper();
                    } else {
                        throw new Exception("Unknown piece type.");
                    }
                    EntityScript script = entity.GetComponent<EntityScript>();
                    if (state.Is(x, y, z, DocurioEntity.Black)) {
                        entity.GetComponent<EntityScript>().BecomeBlack();
                    }
                    entity.transform.localPosition = new Vector3(x, entityHeight * z, y);
                    entityScripts[x, y, z] = script;
                }
            }
        }
        aiControl = new bool[] { false, true };
    }

    void Update() {
        if (state.win > -1) {
            return;
        }
        if (aiControl[state.toPlay]) {
            if (AI.status == AIStatus.Ready) {
                AI.Start(state, (int)levelInfo.difficulty);
            } else if (!IsAnimating() && AI.status == AIStatus.Done) {
                ExecuteMove(AI.move);
                AI.status = AIStatus.Ready;
            }
            return;
        }
        if (IsAnimating()) {
            return;
        }
        if (!UpdateSelectMove()) {
            UpdateSelectUnit();
        }
    }
    bool IsAnimating() {
        for (int x = 0; x < state.xSize; x++) {
            for (int y = 0; y < state.ySize; y++) {
                for (int z = 0; z < state.zSize; z++) {
                    if (entityScripts[x, y, z] != null && entityScripts[x, y, z].IsAnimating()) {
                        return true;
                    }
                }
            }
        }
        return false;
    } 

    bool UpdateSelectMove() {
        if (!Input.GetMouseButtonDown(0)) {
            return false;
        }
        Collider collider = Util.GetMouseCollider(layerMaskMove);
        if (collider == null || !selectMoveObjects.ContainsKey(collider)) {
            return false;
        }
        DocurioMove move = selectMoveObjects[collider];
        ExecuteMove(move);
        ClearSelection();
        return true;
    }
    void ExecuteMove(DocurioMove move) {
        List<Tuple<Int3, Int3>> slides = new List<Tuple<Int3, Int3>>();
        List<Int3> destroyedUnits = new List<Int3>();
        state.Execute(move, slides, destroyedUnits);
        foreach (Int3 d in destroyedUnits) {
            Destroy(entityScripts[d.x, d.y, d.z].gameObject); // TODO: VFX
            entityScripts[d.x, d.y, d.z] = null;
        }
        EntityScript unitScript = entityScripts[move.from.x, move.from.y, move.from.z];
        if (!move.snipe && move.from != move.to) {
            entityScripts[move.to.x, move.to.y, move.to.z] = unitScript;
            entityScripts[move.from.x, move.from.y, move.from.z] = null;
            if (state.Is(move.to, DocurioEntity.Sniper)) {
                unitScript.AnimateTeleport(move);
            } else {
                unitScript.AnimateLinearMove(state, move);
            }
        }
        Dictionary<EntityScript, Tuple<Int3, Int3>> scriptToSlide = new Dictionary<EntityScript, Tuple<Int3, Int3>>();
        foreach (Tuple<Int3, Int3> slide in slides) {
            EntityScript slidScript = entityScripts[slide.Item1.x, slide.Item1.y, slide.Item1.z];
            entityScripts[slide.Item2.x, slide.Item2.y, slide.Item2.z] = slidScript;
            entityScripts[slide.Item1.x, slide.Item1.y, slide.Item1.z] = null;
            scriptToSlide.Add(slidScript, slide);
        }
        if (scriptToSlide.Count > 0) {
            unitScript.AnimatePushes(move.to, scriptToSlide);
        }
    }
    void UpdateSelectUnit() {
        if (!Input.GetMouseButtonDown(0)) {
            return;
        }
        ClearSelection();
        Collider collider = Util.GetMouseCollider(layerMaskSwitch);
        if (collider == null) {
            return;
        }
        Int3 from = Util.FindIndex3(entityScripts, collider.GetComponent<EntityScript>());
        DocurioEntity color = state.toPlay == 0 ? DocurioEntity.White : DocurioEntity.Black;
        if (!state.Is(from, color)) {
            return;
        }
        List<DocurioMove> moves = new List<DocurioMove>();
        state.AddMoves(moves, from);
        foreach (DocurioMove move in moves) {
            if (!move.pushDirection.IsZero()) {
                GameObject selectBlock = Instantiate(selectBlockPrefab, transform);
                selectBlock.transform.localPosition = new Vector3(move.to.x, move.to.z * entityHeight, move.to.y);
                float thetaY = Mathf.Atan2(move.pushDirection.y, -move.pushDirection.x) * Mathf.Rad2Deg;
                selectBlock.transform.localRotation = Quaternion.Euler(0, thetaY, 0);
                selectMoveObjects[selectBlock.GetComponentInChildren<Collider>()] = move;
            } else {
                GameObject selectTile = Instantiate(selectTilePrefab, transform);
                if (!state.Is(move.to, DocurioEntity.Empty)) {
                    selectTile.GetComponent<SelectTileScript>().CaptureColor();
                }
                selectTile.transform.localPosition = new Vector3(move.to.x, move.to.z * entityHeight, move.to.y);
                selectMoveObjects[selectTile.GetComponent<Collider>()] = move;
            }
        }
    }
    void ClearSelection() {
        foreach (var kvp in selectMoveObjects) {
            GameObject go = kvp.Key.gameObject;
            while (true) {
                GameObject parent = go.transform.parent.gameObject;
                if (parent == gameObject) {
                    break;
                }
                go = parent;
            }
            Destroy(go);
        }
        selectMoveObjects.Clear();
    }
}
