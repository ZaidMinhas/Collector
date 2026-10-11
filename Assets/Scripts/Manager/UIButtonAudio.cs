using UnityEngine;
using UnityEngine.UI;

namespace Manager
{
    [RequireComponent(typeof(Button))]
    public class UIButtonAudio : MonoBehaviour
    {
        private Button button;
        private Button.ButtonClickedEvent registeredEvent;
        private bool handledByAction;

        private void Awake() => button = GetComponent<Button>();
        private void OnEnable() => Register();
        private void LateUpdate() => Register();

        public void SetHandledByAction()
        {
            // PauseMenu.Resume supplies the closing sound for both Escape and the button.
            handledByAction = true;
            Unregister();
        }

        private void Register()
        {
            if (handledByAction || button == null || registeredEvent == button.onClick) return;
            Unregister();
            registeredEvent = button.onClick;
            // Static callback can still play when another listener changes scenes.
            registeredEvent.AddListener(UIAudio.PlayCool);
        }

        private void Unregister()
        {
            if (registeredEvent != null) registeredEvent.RemoveListener(UIAudio.PlayCool);
            registeredEvent = null;
        }

        private void OnDisable() => Unregister();
    }
}
