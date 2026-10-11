using System;
using UnityEngine;

public class PopupText : MonoBehaviour
{
    public bool isPlayOnLoad = true;

    public void Start()
    {
        if (isPlayOnLoad)
        {
            Time.timeScale = 0;
        }
    }


    public void Resume()
    {
        Time.timeScale = 1;
    }
}
