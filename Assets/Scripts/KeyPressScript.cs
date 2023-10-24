using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Utility
{
    public class KeyPressScript : MonoBehaviour
    {
        [SerializeField] private UnityEvent KeyPressed;
        [SerializeField] private UnityEvent KeyPressedAgain;

        [Tooltip("The keybinding to trigger the action, you can alternatively trigger the action through another event.")]
        [SerializeField] private InputAction actionToActivate;

        private bool isActive = false;

        [Tooltip("If left false, it will only register the KeyPressed, it will not check to see if it has been pressed before.")]
        [SerializeField] private bool performDifferentActionWhenPressedBefore = false;

        [Tooltip("Whether or not pauzing the game will affect the script.")]
        [SerializeField] private bool affectedByPauze = false;
        private bool gamePaused = false;

        private void OnEnable()
        {
            actionToActivate.performed += ctx => PerformAction();
            actionToActivate.Enable();
            EventManager.AddListener(EventType.Pause, () => gamePaused = true);
            EventManager.AddListener(EventType.UnPause, () => gamePaused = false);
        }

        public void PerformAction()
        {
            if (affectedByPauze && gamePaused) { return; }

            if (performDifferentActionWhenPressedBefore)
            {
                if (isActive)
                {
                    isActive = false;
                    KeyPressedAgain?.Invoke();
                }
                else
                {
                    isActive = true;
                    KeyPressed?.Invoke();
                }
            }
            else
            {
                KeyPressed?.Invoke();
            }
        }
    }
}