using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code {
    public static class MoveGeneration {
        static Int2[] CARDINALS = new Int2[] { new Int2(-1, 0), new Int2(1, 0), new Int2(0, -1), new Int2(0, 1) };
        static Int2[] CARDINALS_AND_ORDINALS = new Int2[] { new Int2(-1, -1), new Int2(-1, 0), new Int2(-1, 1), new Int2(0, -1), new Int2(0, 1), new Int2(1, -1), new Int2(1, 0), new Int2(1, 1) };

        public static void AddMoves(this DocurioState state, List<DocurioMove> moves, Int3 from) {
            DocurioEntity piece = state.Get(from);
            if ((piece & DocurioEntity.King) > 0) {
                AddCompassMoves(state, moves, from, true, false, false);
            } else if ((piece & DocurioEntity.Pusher) > 0) {
                AddCompassMoves(state, moves, from, false, true, true);
            } else {
                throw new Exception("Could not find piece at " + from);
            }
        }

        public static void AddCompassMoves(DocurioState state, List<DocurioMove> moves, Int3 from, bool includeDiagonal, bool anyDistance, bool canClimb) {
            foreach (Int2 direction in includeDiagonal ? CARDINALS_AND_ORDINALS : CARDINALS) {
                int x = from.x + direction.x;
                int y = from.y + direction.y;
                int lastZ = from.z;
                bool climbLeftInThisDirection = canClimb;
                while (true) {
                    if (x < 0 || x >= state.board.GetLength(0) || y < 0 || y >= state.board.GetLength(1)) {
                        break;
                    }
                    int z = state.GroundZ(x, y);
                    if (z > lastZ && !climbLeftInThisDirection) {
                        break;
                    }
                    if (z > lastZ + 1) {
                        // You can only climb one block.
                        break;
                    }
                    // Check for blocks blocking diagonal movement.
                    if (direction.x != 0 && direction.y != 0) {
                        int maxTravelZ = Mathf.Max(z, lastZ);
                        int maxAdjacentZ = Mathf.Max(state.GroundZ(x - direction.x, y), state.GroundZ(x, y - direction.y));
                        if (maxAdjacentZ > maxTravelZ) {
                            break;
                        }
                    }
                    if (z < lastZ) {
                        // You can't descend and ascend in the same move.
                        climbLeftInThisDirection = false;
                    }
                    Int3 space = new Int3(x, y, z);
                    // Check for other pieces in the space.
                    if (!state.Is(space, DocurioEntity.Empty)) {
                        break;
                        // TODO: Capture.
                    }
                    // Add move.
                    moves.Add(new DocurioMove(from, space));
                    if (!anyDistance) {
                        break;
                    }
                    if (z > lastZ) {
                        // Units can't continue moving after climbing.
                        break;
                    }
                    if (z < lastZ - 1) {
                        // Units can't continue moving after dropping 2 or more blocks down.
                        break;
                    }
                    x += direction.x;
                    y += direction.y;
                    lastZ = z;
                }
            }
        }

        public static int GroundZ(this DocurioState state, int x, int y) {
            for (int z = 0; z < state.board.GetLength(2); z++) {
                if (!state.Is(x, y, z, DocurioEntity.Block)) {
                    return z;
                }
            }
            throw new Exception("A block got onto the top Z level.");
        }
    }
}
