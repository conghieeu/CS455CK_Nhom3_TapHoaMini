using System;
using System.Collections;
using UnityEngine;

public enum Platform
{
    Standalone,
    Android,
    iOS
}

public class GameSystem : GameBehavior
{
    [Header("GAME SYSTEM")]
    public Platform CurrentPlatform;
}

