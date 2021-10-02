using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code {
    public class DocurioState {
        static int MAX_HEIGHT = 3;

        public int xSize, ySize, zSize;
        public DocurioEntity[,,] board;
        public int toPlay;

        public DocurioState(int size) {
            xSize = size;
            ySize = size;
            zSize = MAX_HEIGHT;
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
                        } else if (UnityEngine.Random.value < .3f) {
                            board[x, y, 1] = DocurioEntity.Pusher | (UnityEngine.Random.value < .5f ? DocurioEntity.White : DocurioEntity.Black);
                        }
                    }
                }
            }
            toPlay = 0;
        }

        public DocurioEntity Get(Int3 coor) {
            return board[coor.x, coor.y, coor.z];
        }
        public bool Is(int x, int y, int z, DocurioEntity type) {
            if (type == DocurioEntity.Empty) {
                return board[x, y, z] == DocurioEntity.Empty;
            }
            return (board[x, y, z] & type) != 0;
        }
        public bool Is(Int3 coor, DocurioEntity type) {
            return Is(coor.x, coor.y, coor.z, type);
        }
        public int GroundZ(int x, int y) {
            for (int z = 0; z < zSize; z++) {
                if (!Is(x, y, z, DocurioEntity.Block)) {
                    return z;
                }
            }
            throw new Exception("A block got onto the top Z level.");
        }

        public void Execute(DocurioMove move, List<Tuple<Int3, Int3>> slides = null, List<Int3> destroyedUnits = null) {
            toPlay = (toPlay + 1) % 2;
            Int3 from = move.from, to = move.to;
            Int2 pushDirection = move.pushDirection;
            if (destroyedUnits != null && from != to && !Is(to, DocurioEntity.Empty)) {
                destroyedUnits.Add(to);
            }
            MoveEntity(from, to);
            if (!pushDirection.IsZero()) {
                // Find the space behind the columns being pushed.
                int behindX = to.x + pushDirection.x * 2, behindY = to.y + pushDirection.y * 2, behindZ = GroundZ(behindX, behindY);
                int numColumnsPushed = 1;
                while (behindZ > to.z) {
                    behindX += pushDirection.x;
                    behindY += pushDirection.y;
                    behindZ = GroundZ(behindX, behindY);
                    numColumnsPushed++;
                }
                // Find how far to push the columns.
                int destX = behindX, destY = behindY, destZ = GroundZ(destX, destY);
                int columnsOverHole = 0;
                while (true) {
                    if (destroyedUnits != null && !Is(destX, destY, destZ, DocurioEntity.Empty)) {
                        destroyedUnits.Add(new Int3(destX, destY, destZ));
                    }
                    int nextDestX = destX + pushDirection.x;
                    int nextDestY = destY + pushDirection.y;
                    if (nextDestX < 0 || nextDestX >= xSize || nextDestY < 0 || nextDestY >= ySize || GroundZ(nextDestX, nextDestY) > to.z) { // Hit a wall.
                        break;
                    }
                    if (destZ < to.z) { // Hit a hole.
                        columnsOverHole++;
                        if (columnsOverHole >= numColumnsPushed) {
                            break;
                        }
                    }
                    destX = nextDestX;
                    destY = nextDestY;
                    destZ = GroundZ(destX, destY);
                }
                // Perform the actual pushes.
                int pushX = to.x;
                int pushY = to.y;
                for(; numColumnsPushed > 0; numColumnsPushed--) {
                    behindX -= pushDirection.x;
                    behindY -= pushDirection.y;
                    for (int dz = 0; to.z + dz < zSize && destZ + dz < zSize; dz++) {
                        Int3 pushFromCoor = new Int3(behindX, behindY, to.z + dz);
                        if (Is(pushFromCoor, DocurioEntity.Empty)) {
                            break;
                        }
                        Int3 pushToCoor = new Int3(destX, destY, destZ + dz);
                        MoveEntity(pushFromCoor, pushToCoor);
                        if (slides != null) {
                            slides.Add(new Tuple<Int3, Int3>(pushFromCoor, pushToCoor));
                        }
                    }
                    destX -= pushDirection.x;
                    destY -= pushDirection.y;
                    destZ = GroundZ(destX, destY);
                }
            }
        }
        public void MoveEntity(Int3 from, Int3 to) {
            if (from == to) {
                return;
            }
            board[to.x, to.y, to.z] = board[from.x, from.y, from.z];
            board[from.x, from.y, from.z] = DocurioEntity.Empty;
        }
    }

    [Flags]
    public enum DocurioEntity : int {
        Empty = 0,
        Block = 1,
        White = 2,
        Black = 2 << 1,
        King = 2 << 2,
        Pusher = 2 << 3,
    }

    public struct DocurioMove {
        public Int3 from, to;
        public Int2 pushDirection;

        public DocurioMove(Int3 from, Int3 to, Int2 pushDirection = new Int2()) {
            this.from = from;
            this.to = to;
            this.pushDirection = pushDirection;
        }
    }
}
