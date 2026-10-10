using UnityEngine;
using UnityEngine.InputSystem;

namespace Manager
{
    public class DebugTool : MonoBehaviour
    {
        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            
            if (keyboard.aKey.wasPressedThisFrame) Manager.SceneLoader.LoadGameScene("MainMenu");
            
            if (keyboard.sKey.wasPressedThisFrame) Manager.SceneLoader.LoadGameScene("SettingMenu");
        }
    }
}