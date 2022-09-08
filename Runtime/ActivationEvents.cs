using UnityEngine;
using UnityEngine.Events;

/*
 MIT License

Copyright (c) 2022 Kitbashery

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.



Need support or additional features? Please visit https://kitbashery.com/
*/

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