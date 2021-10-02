using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardScript : MonoBehaviour
{
    public GameObject tilePrefab, blockPrefab, kingPrefab;
    float entityScale;

    DocurioState state;
    GameObject[,] tileObjects;
    GameObject[,,] entityObjects;

    void Start() {
        int size = 7;
        state = new DocurioState(size);
        tileObjects = new GameObject[size, size];
        entityObjects = new GameObject[size, size, state.board.GetLength(2)];
        float mid = (size - 1) / 2;
        entityScale = blockPrefab.transform.localScale.y;
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                GameObject tile = Instantiate(tilePrefab, transform);
                tile.transform.localPosition = new Vector3(x - mid, 0, y - mid);
                tileObjects[x, y] = tile;
                for (int z = 0; z < state.board.GetLength(2); z++) {
                    GameObject entity = null;
                    if (state.board[x, y, z] == DocurioEntity.Block) {
                        entity = Instantiate(blockPrefab, transform);
                    } else if (state.board[x, y, z] == DocurioEntity.King) {
                        entity = Instantiate(kingPrefab, transform);
                    } else {
                        break;
                    }
                    entity.transform.localPosition = new Vector3(x - mid, entityScale * z, y - mid);
                    entityObjects[x, y, z] = entity;
                }
            }
        }
    }

    void Update() {
        
    }
}
