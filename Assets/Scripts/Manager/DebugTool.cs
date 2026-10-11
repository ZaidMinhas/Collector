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
            
            if (keyboard.yKey.wasPressedThisFrame) Manager.SceneLoader.LoadGameScene("OptionSetting");

            if (keyboard.eKey.wasPressedThisFrame)
            {
                AudioClip clip = Resources.Load<AudioClip>("Musics/ProbablyLevel2(WIP)");
                AudioManager.Instance.PlayMusic(clip);
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                AudioClip clip = Resources.Load<AudioClip>("Sfx/sound");
                AudioManager.Instance.PlayNonEnemySFX(clip);
            }
        }
    }
}
