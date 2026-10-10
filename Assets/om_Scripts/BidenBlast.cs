using TMPro;
using UnityEngine;

public class BidenBlast : MonoBehaviour
{
    public TextMeshProUGUI shootText;
    public TextMeshProUGUI abductText;

    private void Start()
    {
        shootText.enabled = false;
        abductText.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) { shootText.enabled = true; abductText.enabled = false; }
        if (Input.GetKeyUp(KeyCode.Mouse0)) shootText.enabled = false;
    }
}
