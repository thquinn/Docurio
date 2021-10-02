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
        static Dictionary<DocurioEntity, UnitProperties> UNIT_PROPERTIES = new Dictionary<DocurioEntity, UnitProperties>{
            { DocurioEntity.King, new UnitProperties(){ diagonal = true, distance = 1, canClimb = false } },
            { DocurioEntity.Pusher, new UnitProperties(){ canPush = true } },
        };

        public static void AddMoves(this DocurioState state, List<DocurioMove> moves, Int3 from) {
            DocurioEntity piece = state.Get(from);
            if ((piece & DocurioEntity.King) > 0) {
                AddCompassMoves(state, moves, from, UNIT_PROPERTIES[DocurioEntity.King]);
            } else if ((piece & DocurioEntity.Pusher) > 0) {
                AddCompassMoves(state, moves, from, UNIT_PROPERTIES[DocurioEntity.Pusher]);
            } else {
                throw new Exception("Could not find piece at " + from);
            }
        }

        public static void AddCompassMoves(DocurioState state, List<DocurioMove> moves, Int3 from, UnitProperties unitProperties) {
            foreach (Int2 direction in unitProperties.diagonal ? CARDINALS_AND_ORDINALS : CARDINALS) {
                int x = from.x + direction.x;
                int y = from.y + direction.y;
                int lastZ = from.z;
                int movesLeftInThisDirection = unitProperties.distance;
                bool climbLeftInThisDirection = unitProperties.canClimb;
                while (true) {
                    if (x < 0 || x >= state.xSize || y < 0 || y >= state.ySize) {
                        break;
                    }
                    bool diagonal = direction.x != 0 && direction.y != 0;
                    int z = state.GroundZ(x, y);
                    
                    if (unitProperties.canPush && z > lastZ && !diagonal) {
                        // Pushing.
                        int checkX = x + direction.x, checkY = y + direction.y;
                        while (checkX >= 0 && checkX < state.xSize && checkY >= 0 && checkY < state.ySize) {
                            if (state.GroundZ(checkX, checkY) <= lastZ) {
                                moves.Add(new DocurioMove(from, new Int3(x - direction.x, y - direction.y, lastZ), direction));
                                break;
                            }
                            checkX += direction.x;
                            checkY += direction.y;
                        }
                    }
                    if (--movesLeftInThisDirection < 0) {
                        break;
                    }
                    if (z > lastZ && !climbLeftInThisDirection) {
                        break;
                    }
                    if (z > lastZ + 1) {
                        // You can only climb one block.
                        break;
                    }
                    if (diagonal) {
                        // Check for blocks blocking diagonal movement.
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
                    // Check for other pieces in the space.
                    Int3 space = new Int3(x, y, z);
                    if (!state.Is(space, DocurioEntity.Empty)) {
                        break;
                        // TODO: Capture.
                    }
                    // Add move.
                    moves.Add(new DocurioMove(from, space));
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
    }

    public class UnitProperties {
        public bool diagonal = false;
        public int distance = 99;
        public bool canClimb = true;
        public bool canPush = false;
    }
}
