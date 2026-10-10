using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class SceneLoader : MonoBehaviour
    {
        public static void LoadGameScene(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }

        public static void QuitGame()
        {
            Debug.Log("Quit Game");
            Application.Quit();
        }
    }
}

