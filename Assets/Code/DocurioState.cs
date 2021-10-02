using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code {
    public class DocurioState {
        static int MAX_HEIGHT = 3;

        public DocurioEntity[,,] board;
        public int playerToMove = 0;

        public DocurioState(int size) {
            bool kingPlaced = false;
            board = new DocurioEntity[size, size, MAX_HEIGHT];
            for (int x = 0; x < size; x++) {
                for (int y = 0; y < size; y++) {
                    if (UnityEngine.Random.value < .5f) {
                        board[x, y, 0] = DocurioEntity.Block;
                        if (!kingPlaced) {
                            board[x, y, 1] = DocurioEntity.King;
                            kingPlaced = true;
                        } else if (UnityEngine.Random.value < .25f) {
                            board[x, y, 1] = DocurioEntity.Block;
                        }
                    }
                }
            }
        }
    }

    public enum DocurioEntity {
        Empty, Block, King
    }
}
