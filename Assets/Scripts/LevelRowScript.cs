using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelRowScript : MonoBehaviour
{
    public TextMeshProUGUI tmpNumber, tmpName;
    public ButtonScript[] buttons;

    public void Set(int index, string name, string layout) {
        tmpNumber.text = (index + 1) + ".";
        tmpName.text = name;
        buttons[0].SetLevelInfo(new LevelInfo(index, LevelDifficulty.Easy, layout));
        buttons[1].SetLevelInfo(new LevelInfo(index, LevelDifficulty.Standard, layout));
        buttons[2].SetLevelInfo(new LevelInfo(index, LevelDifficulty.Hard, layout));
    }
}
