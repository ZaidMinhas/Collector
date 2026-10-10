using TMPro;
using UnityEngine;

public class BidenBlast : MonoBehaviour
{
    public TextMeshProUGUI showText;

    private void Start()
    {
        showText.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) showText.enabled = true;
        if (Input.GetKeyUp(KeyCode.Mouse0)) showText.enabled = false;
    }
}
