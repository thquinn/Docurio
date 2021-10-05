using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code {
    public class LevelInfo {
        public static HashSet<string> NON_AI_CENTRIC_LEVELS = new HashSet<string>() {
            "Bridge"
        };

        public int index;
        public LevelDifficulty difficulty;
        public string layout;

        public LevelInfo(int index, LevelDifficulty difficulty, string layout) {
            this.index = index;
            this.difficulty = difficulty;
            this.layout = layout;
        }
    }

    public enum LevelDifficulty {
        Easy = 1000,
        Standard = 5000,
        Hard = 10000
    }
}
