using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace Assets.Code {
    public class AI {
        static Thread thread;
        public static AIStatus status = AIStatus.Ready;
        public static DocurioMove move;
        public static string statusString;

        public static void Start(DocurioState state, int rollouts) {
            status = AIStatus.Working;
            thread = new Thread(() => {
                MCTS mcts = new MCTS(new DocurioState(state));
                mcts.Rollout(rollouts);
                move = mcts.rootNode.MoveWithMostRollouts();
                status = AIStatus.Done;
            });
            thread.Start();
        }
    }

    public class MCTS {
        public MCTSNode rootNode;
        DocurioState rootState;
        System.Random random;
        int draws = 0;

        public MCTS(DocurioState state) {
            random = new System.Random(0);
            rootNode = new MCTSNode(null, state, random);
            rootState = state;
        }

        public void Rollout(int n) {
            Profiler.BeginThreadProfiling("My Threads", "My Thread");
            if (rootNode.children.Length == 1) {
                Rollout();
                return;
            }
            while (rootNode.rollouts < n) {
                Rollout();
                AI.statusString = rootNode.rollouts + " rollouts";
            }
            Profiler.EndThreadProfiling();
        }
        public void Rollout() {
            // Selection.
            MCTSNode currentNode = rootNode;
            DocurioState currentState = new DocurioState(rootState);
            while (true) {
                MCTSMove move = currentNode.GetChild();
                if (move.child == null) {
                    break;
                }
                currentNode = move.child;
                currentState.Execute(move.move);
            }
            // Expansion.
            currentNode = currentNode.Expand(currentState, random);
            // Simulation.
            while (currentState.win == -1) {
                if (currentState.moves > rootState.moves + 100) {
                    draws++;
                    break;
                }
                List<DocurioMove> moves = currentState.AllMoves();
                if (moves.Count == 0) {
                    currentState.win = 1 - currentState.toPlay;
                    break;
                }
                int i = random.Next(moves.Count);
                currentState.Execute(moves[i]);
            }
            // Backpropagate.
            while (currentNode.parent != null) {
                currentNode.rollouts++;
                if (currentState.win == currentNode.parent.toPlay) {
                    currentNode.totalReward++;
                }
                currentNode = currentNode.parent;
            }
            rootNode.rollouts++;
        }
    }

    public class MCTSNode {
        public readonly static double EXPLORATION = Math.Sqrt(2);

        public MCTSNode parent;
        List<DocurioMove> moves;
        public MCTSNode[] children;
        int expandedChildrenCount;
        public int toPlay;
        public int rollouts;
        public float totalReward;

        public MCTSNode(MCTSNode parent, DocurioState state, System.Random random) {
            this.parent = parent;
            moves = state.AllMoves();
            Util.ShuffleList(random, moves);
            children = new MCTSNode[moves.Count];
            toPlay = state.toPlay;
        }

        public MCTSMove GetChild() {
            if (children.Length == 0 || expandedChildrenCount < children.Length) {
                return new MCTSMove();
            }
            double highestUCT = double.MinValue;
            int highestIndex = -1;
            double lnSimulations = Math.Log(rollouts);
            for (int i = 0; i < expandedChildrenCount; i++) {
                MCTSNode child = children[i];
                double uct = child.totalReward / child.rollouts + EXPLORATION * Math.Sqrt(lnSimulations / child.rollouts);
                if (uct >= highestUCT) {
                    highestUCT = uct;
                    highestIndex = i;
                }
            }
            return new MCTSMove(moves[highestIndex], children[highestIndex]);
        }

        public MCTSNode Expand(DocurioState state, System.Random random) {
            if (children.Length == 0) return this;
            DocurioMove move = moves[expandedChildrenCount];
            state.Execute(move);
            MCTSNode child = new MCTSNode(this, state, random);
            children[expandedChildrenCount++] = child;
            return child;
        }

        public DocurioMove MoveWithMostRollouts() {
            DocurioMove bestMove = DocurioMove.None;
            int mostRollouts = -1;
            for (int i = 0; i < children.Length; i++) {
                if (children[i] == null) {
                    continue;
                }
                if (children[i].rollouts > mostRollouts) {
                    bestMove = moves[i];
                    mostRollouts = children[i].rollouts;
                }
            }
            return bestMove;
        }
    }

    public struct MCTSMove {
        public DocurioMove move;
        public MCTSNode child;

        public MCTSMove(DocurioMove move, MCTSNode child) {
            this.move = move;
            this.child = child;
        }
    }

    public enum AIStatus {
        Ready, Working, Done
    }
}
