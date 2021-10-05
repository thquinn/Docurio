using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelRowScript : MonoBehaviour
{
    public TextMeshProUGUI tmpNumber, tmpName;
    public ButtonScript[] buttons;

    public void Set(int index, string name, string layoutEasy, string layoutStandard, string layoutHard) {
        tmpNumber.text = index + ".";
        tmpName.text = name;
        bool allEasy = LevelInfo.NON_AI_CENTRIC_LEVELS.Contains(name);
        buttons[0].SetLevelInfo(new LevelInfo(index - 1, LevelDifficulty.Easy, layoutEasy));
        buttons[1].SetLevelInfo(new LevelInfo(index - 1, allEasy ? LevelDifficulty.Easy : LevelDifficulty.Standard, layoutStandard));
        buttons[2].SetLevelInfo(new LevelInfo(index - 1, allEasy ? LevelDifficulty.Easy : LevelDifficulty.Hard, layoutHard));
    }
}
