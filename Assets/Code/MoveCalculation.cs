﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code {
    public static class MoveCalculation {
        static Int2[] CARDINALS = new Int2[] { new Int2(-1, 0), new Int2(1, 0), new Int2(0, -1), new Int2(0, 1) };
        static Int2[] CARDINALS_AND_ORDINALS = new Int2[] { new Int2(-1, -1), new Int2(-1, 0), new Int2(-1, 1), new Int2(0, -1), new Int2(0, 1), new Int2(1, -1), new Int2(1, 0), new Int2(1, 1) };

        public static void AddMoves(this DocurioState state, List<DocurioMove> moves, Int3 from) {
            DocurioEntity piece = state.Get(from);
            if ((piece & DocurioEntity.King) > 0) {
                AddCompassMoves(state, moves, from, true, false, false);
            } else {
                throw new Exception("Could not find piece at " + from);
            }
        }

        public static void AddCompassMoves(DocurioState state, List<DocurioMove> moves, Int3 from, bool includeDiagonal, bool anyDistance, bool canClimb) {
            foreach (Int2 direction in includeDiagonal ? CARDINALS_AND_ORDINALS : CARDINALS) {
                int x = from.x + direction.x;
                int y = from.y + direction.y;
                int lastZ = from.z;
                while (true) {
                    if (x < 0 || x >= state.board.GetLength(0) || y < 0 || y >= state.board.GetLength(1)) {
                        break;
                    }
                    int z = state.GroundZ(x, y);
                    if (z > lastZ && !canClimb) {
                        break;
                    }
                    // Check for blocks blocking diagonal movement.
                    if (direction.x != 0 && direction.y != 0 && (state.GroundZ(x - direction.x, y) > lastZ || state.GroundZ(x, y - direction.y) > lastZ)) {
                        break;
                    }
                    lastZ = z;
                    // Add move.
                    moves.Add(new DocurioMove(from, new Int3(x, y, z)));
                    if (z > lastZ) {
                        // Units can't continue moving after climbing.
                        break;
                    }
                    if (!anyDistance) {
                        break;
                    }
                    x += direction.x;
                    y += direction.y;
                    // TODO: Capture.
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
