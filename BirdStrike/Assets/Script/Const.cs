using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SIDE
{
    NONE = 0,
    PLAYER,
    ENEMY,
}

public enum ENENY_TYPE
{
    NORMAL_ENEMY,
    // Swaying enemy
    SWING_ENEMY,
    BOSS,
}


public enum GAME_STATUS
{
    READY,
    INGAME,
    OVER
}
