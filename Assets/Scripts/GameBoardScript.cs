using Assets.Code;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardScript : MonoBehaviour
{
    public GameObject tilePrefab, blockPrefab, unitPrefab, selectTilePrefab, selectBlockPrefab;
    public LayerMask layerMaskSwitch, layerMaskMove;
    public static float entityHeight;

    DocurioState state;
    GameObject[,] tileObjects;
    GameObject[,,] entityObjects;
    // Unit and move selection.
    bool[] aiControl;
    Dictionary<Collider, DocurioMove> selectMoveObjects = new Dictionary<Collider, DocurioMove>();

    void Start() {
        int size = 7;
        transform.localPosition = new Vector3((size - 1) / -2, 0, (size - 1) / -2);
        state = new DocurioState(size);
        tileObjects = new GameObject[size, size];
        entityObjects = new GameObject[size, size, state.zSize];
        entityHeight = blockPrefab.transform.localScale.y;
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
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
                    } else {
                        throw new Exception("Unknown piece type.");
                    }
                    if (state.Is(x, y, z, DocurioEntity.Black)) {
                        entity.GetComponent<EntityScript>().BecomeBlack();
                    }
                    entity.transform.localPosition = new Vector3(x, entityHeight * z, y);
                    entityObjects[x, y, z] = entity;
                }
            }
        }
        aiControl = new bool[] { false, false };
    }

    void Update() {
        if (aiControl[state.toPlay]) {
            return;
        }
        if (!UpdateSelectMove()) {
            UpdateSelectUnit();
        }
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
            Destroy(entityObjects[d.x, d.y, d.z]); // TODO: VFX
            entityObjects[d.x, d.y, d.z] = null;
        }
        GameObject unit = entityObjects[move.from.x, move.from.y, move.from.z];
        EntityScript unitScript = unit.GetComponent<EntityScript>();
        if (move.from != move.to) {
            entityObjects[move.to.x, move.to.y, move.to.z] = unit;
            entityObjects[move.from.x, move.from.y, move.from.z] = null;
            unitScript.AnimateLinearMove(state, move);
        }
        Dictionary<EntityScript, Tuple<Int3, Int3>> scriptToSlide = new Dictionary<EntityScript, Tuple<Int3, Int3>>();
        foreach (Tuple<Int3, Int3> slide in slides) {
            GameObject slidEntity = entityObjects[slide.Item1.x, slide.Item1.y, slide.Item1.z];
            entityObjects[slide.Item2.x, slide.Item2.y, slide.Item2.z] = slidEntity;
            entityObjects[slide.Item1.x, slide.Item1.y, slide.Item1.z] = null;
            scriptToSlide.Add(slidEntity.GetComponent<EntityScript>(), slide);
        }
        unitScript.AnimatePushes(move.to, scriptToSlide);
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
        Int3 from = Util.FindIndex3(entityObjects, collider.gameObject);
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
