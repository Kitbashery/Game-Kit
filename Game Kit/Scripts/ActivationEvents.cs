using UnityEngine;
using UnityEngine.Events;

namespace Kitbashery.Gameplay
{
    /// <summary>
    /// Exposes <see cref="UnityEvent"/>s to the inspector that are invoked during their corralating activation messages.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/game-kit/activation-events.html")]
    [DisallowMultipleComponent]
    [AddComponentMenu("Kitbashery/Event/Activation Events")]
    public class ActivationEvents : MonoBehaviour
    {
        [Header("Called when a script instance is being loaded.")]
        public UnityEvent onAwake;
        [Header("Called before the first frame update.")]
        public UnityEvent onStart;
        [Header("Called when the object becomes active and enabled.")]
        public UnityEvent onEnable;
        [Header("Called when the object becomes disabled or inactive.")]
        public UnityEvent onDisable;
        [Header("Called when the MonoBehaviour will be destroyed.")]
        public UnityEvent onDestroy;

        private void Awake()
        {
            onAwake.Invoke();
        }

        private void OnEnable()
        {
            onEnable.Invoke();
        }

        private void Start()
        {
            onStart.Invoke();
        }

        private void OnDisable()
        {
            onDisable.Invoke();
        }

        private void OnDestroy()
        {
            onDestroy.Invoke();
        }
    }
}