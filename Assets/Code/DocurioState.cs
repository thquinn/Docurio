using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code {
    public class DocurioState {
        static readonly Dictionary<char, DocurioEntity> CHAR_TO_PIECE = new Dictionary<char, DocurioEntity>() {
            { 'K', DocurioEntity.White | DocurioEntity.King },
            { 'k', DocurioEntity.Black | DocurioEntity.King },
            { 'P', DocurioEntity.White | DocurioEntity.Pusher },
            { 'p', DocurioEntity.Black | DocurioEntity.Pusher },
        };

        public int xSize, ySize, zSize;
        public DocurioEntity[,,] board;
        public int toPlay, whitePieces, blackPieces, win, moves;

        public DocurioState(TextAsset text) {
            string[] lines = Regex.Split(text.text, "\r\n|\n|\r");
            xSize = lines[0].Length;
            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i];
                int firstChar = line[0] - '0';
                if (firstChar < 0 || firstChar > 9) {
                    break;
                }
                ySize++;
                for (int j = 0; j < line.Length; j++) {
                    zSize = Mathf.Max(zSize, line[j] - '0');
                }
            }
            zSize += 2;
            board = new DocurioEntity[xSize, ySize, zSize];
            for (int x = 0; x < xSize; x++) {
                for (int y = 0; y < ySize; y++) {
                    int height = lines[y][x] - '0';
                    for (int z = 0; z < height; z++) {
                        board[x, y, z] = DocurioEntity.Block;
                    }
                }
            }
            for (int i = ySize; i < lines.Length; i++) {
                string line = lines[i];
                DocurioEntity piece = CHAR_TO_PIECE[line[0]];
                string[] tokens = line.Substring(1).Split(',');
                int x = int.Parse(tokens[0]);
                int y = int.Parse(tokens[1]);
                int z = GroundZ(x, y);
                board[x, y, z] = piece;
                if (Is(x, y, z, DocurioEntity.White)) {
                    whitePieces++;
                } else if (Is(x, y, z, DocurioEntity.Black)) {
                    blackPieces++;
                }
            }
            toPlay = 0;
            win = -1;
        }
        public DocurioState(DocurioState other) {
            xSize = other.xSize;
            ySize = other.ySize;
            zSize = other.zSize;
            board = new DocurioEntity[xSize, ySize, zSize];
            Array.Copy(other.board, board, xSize * ySize * zSize);
            toPlay = other.toPlay;
            whitePieces = other.whitePieces;
            blackPieces = other.blackPieces;
            win = other.win;
            moves = other.moves;
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
            moves++;
            Int3 from = move.from, to = move.to;
            Int2 pushDirection = move.pushDirection;
            if (destroyedUnits != null && from != to && !Is(to, DocurioEntity.Empty)) {
                Capture(to);
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
                        Int3 coor = new Int3(destX, destY, destZ);
                        Capture(coor);
                        destroyedUnits.Add(coor);
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
            toPlay = (toPlay + 1) % 2;
            if (!this.MoveExists()) {
                win = 1 - toPlay;
            }
        }
        public void MoveEntity(Int3 from, Int3 to) {
            if (from == to) {
                return;
            }
            board[to.x, to.y, to.z] = board[from.x, from.y, from.z];
            board[from.x, from.y, from.z] = DocurioEntity.Empty;
        }
        public void Capture(Int3 coor) {
            Debug.Assert(Is(coor, DocurioEntity.White) || Is(coor, DocurioEntity.Black));
            if (Is(coor, DocurioEntity.White)) {
                if (--whitePieces == 0 || Is(coor, DocurioEntity.King)) {
                    win = 1;
                }
            } else {
                if (--blackPieces == 0 || Is(coor, DocurioEntity.King)) {
                    win = 0;
                }
            }
        }

        static readonly HashBuilder hashBuilder = new HashBuilder();
        public override int GetHashCode() {
            hashBuilder.Clear();
            for (int x = 0; x < xSize; x++) {
                for (int y = 0; y < ySize; y++) {
                    int z = GroundZ(x, y);
                    hashBuilder.Add(z); // only neccessary if the number of blocks on the board can change
                    hashBuilder.Add(board[x, y, z]);
                }
            }
            return hashBuilder.GetHashCode();
        }
        public override bool Equals(object obj) {
            // Only reliable on boards of equal size.
            if (!(obj is DocurioState)) {
                return false;
            }
            DocurioState other = (DocurioState)obj;
            for (int x = 0; x < xSize; x++) {
                for (int y = 0; y < ySize; y++) {
                    for (int z = zSize - 1; z >= 0; z++) {
                        if (board[x, y, z] != other.board[x, y, z]) {
                            return false;
                        }
                    }
                }
            }
            return true;
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
        public static DocurioMove None = new DocurioMove(Int3.None, Int3.None, Int2.None);

        public Int3 from, to;
        public Int2 pushDirection;

        public DocurioMove(Int3 from, Int3 to, Int2 pushDirection = new Int2()) {
            this.from = from;
            this.to = to;
            this.pushDirection = pushDirection;
        }

        public override int GetHashCode() {
            return from.GetHashCode() ^ to.GetHashCode() ^ pushDirection.GetHashCode();
        }
        public override bool Equals(object obj) {
            if (!(obj is DocurioMove)) {
                return false;
            }
            DocurioMove other = (DocurioMove)obj;
            return other == this;
        }
        public static bool operator ==(DocurioMove a, DocurioMove b) {
            return a.from == b.from && a.to == b.to && a.pushDirection == b.pushDirection;
        }
        public static bool operator !=(DocurioMove a, DocurioMove b) => !(a == b);
    }
}
