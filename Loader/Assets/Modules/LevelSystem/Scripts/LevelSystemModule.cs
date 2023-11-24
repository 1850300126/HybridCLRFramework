using EasyUpdateDemoSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystemModule : ModuleItem
{
    public override void OnLoaded()
    {
        LevelSystem level_system = gameObject.AddComponent<LevelSystem>();

        level_system.OnLoaded();
    } 
}
