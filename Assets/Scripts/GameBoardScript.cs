using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardScript : MonoBehaviour
{
    public GameObject tilePrefab, blockPrefab, unitPrefab, selectTilePrefab;
    public LayerMask layerMaskSwitch, layerMaskMove;
    public static float entityHeight;

    DocurioState state;
    GameObject[,] tileObjects;
    GameObject[,,] entityObjects;
    // Unit and move selection.
    Dictionary<Collider, DocurioMove> selectTileObjects = new Dictionary<Collider, DocurioMove>();

    void Start() {
        int size = 7;
        transform.localPosition = new Vector3((size - 1) / -2, 0, (size - 1) / -2);
        state = new DocurioState(size);
        tileObjects = new GameObject[size, size];
        entityObjects = new GameObject[size, size, state.board.GetLength(2)];
        entityHeight = blockPrefab.transform.localScale.y;
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                GameObject tile = Instantiate(tilePrefab, transform);
                tile.transform.localPosition = new Vector3(x, 0, y);
                tileObjects[x, y] = tile;
                for (int z = 0; z < state.board.GetLength(2); z++) {
                    GameObject entity;
                    if (state.board[x, y, z] == DocurioEntity.Block) {
                        entity = Instantiate(blockPrefab, transform);
                    } else if (state.Is(x, y, z, DocurioEntity.King)) {
                        entity = Instantiate(unitPrefab, transform);
                    } else if (state.Is(x, y, z, DocurioEntity.Pusher)) {
                        entity = Instantiate(unitPrefab, transform);
                        entity.GetComponent<UnitScript>().BecomePusher();
                    } else {
                        break;
                    }
                    entity.transform.localPosition = new Vector3(x, entityHeight * z, y);
                    entityObjects[x, y, z] = entity;
                }
            }
        }
    }

    void Update() {
        if (!UpdateSelectMove()) {
            UpdateSelectUnit();
        }
    }

    bool UpdateSelectMove() {
        if (!Input.GetMouseButtonDown(0)) {
            return false;
        }
        Collider collider = Util.GetMouseCollider(layerMaskMove);
        if (collider == null || !selectTileObjects.ContainsKey(collider)) {
            return false;
        }
        DocurioMove move = selectTileObjects[collider];
        ExecuteMove(move);
        ClearSelection();
        return true;
    }
    void ExecuteMove(DocurioMove move) {
        state.Execute(move);
        GameObject entity = entityObjects[move.from.x, move.from.y, move.from.z];
        entityObjects[move.to.x, move.to.y, move.to.z] = entity;
        entityObjects[move.from.x, move.from.y, move.from.z] = null;
        entity.GetComponent<UnitScript>().AnimateLinearMove(state, move);
        // TODO: Captures, animation.
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
        List<DocurioMove> moves = new List<DocurioMove>();
        state.AddMoves(moves, from);
        foreach (DocurioMove move in moves) {
            GameObject selectTile = Instantiate(selectTilePrefab, transform);
            selectTile.transform.localPosition = new Vector3(move.to.x, move.to.z * entityHeight, move.to.y);
            selectTileObjects[selectTile.GetComponent<Collider>()] = move;
        }
    }
    void ClearSelection() {
        foreach (var kvp in selectTileObjects) {
            Destroy(kvp.Key.gameObject);
        }
        selectTileObjects.Clear();
    }
}
