using EasyUpdateDemoSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class LevelConfig
{
    public string level_name = "";

    public string stage_name = "";

    public string elements_name = "";
}
[Serializable]
public class LevelConfigList
{
    public List<LevelConfig> levels = new List<LevelConfig>();
}

public class LevelSystem : MonoBehaviour
{   
    // 关卡系统api集合
    public Dictionary<string, APISystem.APICallFunction> api_functions = new Dictionary<string, APISystem.APICallFunction>();

    // 当前读取到的关卡配置
    public List<LevelConfig> current_level_list = new List<LevelConfig>();
    public void OnLoaded()
    {   
        // 注册api调用
        // 构建场景类
        api_functions.Add("build_config", BuildConfig);
        api_functions.Add("build_level", BuildLevel);

        // 获取场景信息类
        api_functions.Add("get_elements", GetElements);

        api_functions.Add("unload_level", UnloadLevel);

        api_functions.Add("get_level_info", GetLevelInfo);
        
        APISystem.instance.RegistAPI("level_system", OnLevelSystemAPIFunction);

    }

    public object OnLevelSystemAPIFunction(string function_index, object[] param)
    {
        if (api_functions.ContainsKey(function_index) == true)
        {
            return api_functions[function_index](param);
        }
        return null;
    }

    // 读取所有的配置信息
    public object BuildConfig(object[] param)
    {   
        // 读取所有关卡的config
        List<LevelConfigList> config_list = ConfigSystem.instance.BuildConfig<LevelConfigList>("level_configs");

        // 获取第i关的list
        for (int i = 0; i < config_list.Count; i++)
        {
            for (int j = 0; j < config_list[i].levels.Count; j++)
            {
                // 将第i关的所有LevelConfig类型加入list中
                current_level_list.Add(config_list[i].levels[j]);
            }
        }

        return null;
    }

    // 得到当前关卡需要用到的配置信息
    public LevelConfig current_level_config;
    public CommonInfo current_level_elements_info;
    public object BuildLevel(object[] param)
    {
        current_level_config = GetLevelConfig((string)param[0]);

        if (current_level_config == null) return null;

        Debug.Log(current_level_config.stage_name);

        StageSystem.instance.LoadStage
            (
                current_level_config.stage_name, () =>
                {   
                //     // LoadLevel();
                //     // 游戏UI
                //     LoadGameUI();
                //     // 对象池系统
                //     LoadPoolSystem();
                //     // 关卡element
                //     LoadElements();
                //     // 玩家
                //     LoadPlayer();
                //     // 相机系统
                //     LoadCameraSystem();
                //     // 加载武器模块
                //     LoadWeaponSystem();
                //     // 加载玩家控制模块
                //     LoadPlayerControl();
                //     // 移动点位系统包含人物位置，相机旋转等信息，要在这两个系统之后去加载
                //     LoadMovePointSystem(current_level_config);
                //     // 加载敌人模块
                //     LoadEnemySystem(current_level_config);
                //     // 加载关卡规则模块
                //     LoadLevelRuleSystem(current_level_config);
                }
            );
        return null;
    }

    public LevelConfig GetLevelConfig(string level_name)
    {
        for (int i = 0; i < current_level_list.Count; i++)
        {
            if (current_level_list[i].level_name == level_name)
            {
                return current_level_list[i];
            }
        }
        return null;
    }

    #region 加载除地形以外的其他物体
    public void LoadElements()
    {
        // 加载elements
        BundleInfoSystem.BundleInfoItem data = BundleInfoSystem.instance.GetBundleInfoItem(current_level_config.elements_name, "element");

        current_level_elements_info = BundleInfoSystem.LoadAddressablesPrefabs(data.data, data.name, transform).GetComponent<CommonInfo>();
    }
    // 读取当前角色信息。 （参数后续需要根据存档来变更
    public void LoadPlayer()
    { 
        APISystem.instance.CallAPI("player_system", "build_player", new object[] { "player_01" });
    }
    // 加载相机，相机会寻找Player标签，必须在Player之后加载
    public void LoadCameraSystem()
    {
        APISystem.instance.CallAPI("camera_system", "load_player_camera");
        APISystem.instance.CallAPI("camera_system", "load_bullet_vm");
    }
    // 玩家视角控制器，控制的是相机的点的旋转，所以要等待相机构建完成后加载。
    private void LoadPlayerControl()
    {
        APISystem.instance.CallAPI("player_control", "get_control_trans");
    }
    // 玩家点位系统，用于切换玩家点位
    private void LoadMovePointSystem(LevelConfig levelConfig)
    {
        APISystem.instance.CallAPI("move_point_system", "build_move_point", new object[] { levelConfig.level_name });
    }
    // 武器系统
    private void LoadWeaponSystem()
    {
        APISystem.instance.CallAPI("weapon_system", "build_weapon", new object[] { "pistol_hand_01", "pistol_01" });
    }
    // 敌人系统
    private void LoadEnemySystem(LevelConfig levelConfig)
    {
        // 读取第一关敌人的信息。 （参数后续需要根据存档的关卡来变更
        APISystem.instance.CallAPI("enemy_system", "build_enemy", new object[] { levelConfig.level_name });
    }
    // 对象池已预加载
    private void LoadPoolSystem()
    {
        // ModuleSystem.instance.LoadModule("pool_system");
    }
    // 关卡规则
    private void LoadLevelRuleSystem(LevelConfig levelConfig)
    {
        // 读取第一关敌人的信息。 （参数后续需要根据存档的关卡来变更
        APISystem.instance.CallAPI("level_rule_system", "build_level_rule", new object[] { levelConfig.level_name });
    }

    private void LoadGameUI()
    {
        // ModuleSystem.instance.LoadModule("game_ui");
    } 
    #endregion

    #region 卸载地形等物体
    public object UnloadLevel(object[] param)
    {   
        StartCoroutine(UnloadStage());
        return null;
    }
    // 卸载该关卡的物品
    public IEnumerator UnloadStage()
    {
        SceneManager.UnloadSceneAsync(current_level_config.stage_name);
        Destroy(current_level_elements_info.gameObject);
        current_level_elements_info = null;

        yield return null;

        APISystem.instance.CallAPI("move_point_system", "unload");

        yield return null;

        APISystem.instance.CallAPI("weapon_system", "unload");
        
        yield return null;

        APISystem.instance.CallAPI("player_control", "close_first_control");
        APISystem.instance.CallAPI("player_control", "close_third_control");

        yield return null;

        APISystem.instance.CallAPI("camera_system", "unload");

        yield return null;

        APISystem.instance.CallAPI("player_system", "unload");

        yield return null;

        APISystem.instance.CallAPI("enemy_system", "unload");

        yield return null;

        APISystem.instance.CallAPI("level_rule_system", "unload");
    }

    #endregion

    public CommonInfo GetElements(object[] param)
    {
        return current_level_elements_info;
    }
    public LevelConfig GetLevelInfo(object[] param)
    {
        return current_level_config;
    }
}
 