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
                        if (!kingPlaced && UnityEngine.Random.value < .25f) {
                            board[x, y, 1] = DocurioEntity.King | DocurioEntity.White;
                            kingPlaced = true;
                        } else if (UnityEngine.Random.value < .25f) {
                            board[x, y, 1] = DocurioEntity.Block;
                        }
                    }
                }
            }
        }

        public DocurioEntity Get(Int3 coor) {
            return board[coor.x, coor.y, coor.z];
        }
        public bool Is(int x, int y, int z, DocurioEntity type) {
            return (board[x, y, z] & type) != 0;
        }
        public bool Is(Int3 coor, DocurioEntity type) {
            return Is(coor.x, coor.y, coor.z, type);
        }

        public void Execute(DocurioMove move) {
            board[move.to.x, move.to.y, move.to.z] = board[move.from.x, move.from.y, move.from.z];
            board[move.from.x, move.from.y, move.from.z] = DocurioEntity.Empty;
            // TODO: Capturing.
        }
    }

    [Flags]
    public enum DocurioEntity : int {
        Empty = 0,
        Block = 1,
        White = 2,
        Black = 2 << 1,
        King = 2 << 2,
    }

    public struct DocurioMove {
        public Int3 from, to;

        public DocurioMove(Int3 from, Int3 to) {
            this.from = from;
            this.to = to;
        }
    }
}
