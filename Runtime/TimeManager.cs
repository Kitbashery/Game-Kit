using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace Kitbashery.Gameplay
{
    /// <summary>
    /// Manages play/pause, framerate events and time scale effects.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/game-kit/time-manager.html")]
    [DisallowMultipleComponent]
    [AddComponentMenu("Kitbashery/Event/Time Manager")]
    public class TimeManager : MonoBehaviour
    {
        #region Properties:

        public static TimeManager Instance;

        [Header("Pause Settings:")]
        public KeyCode togglePauseKey = KeyCode.Escape;

        [field: SerializeField]
        public bool pauseAudio { get; set; } = true;
        [field: SerializeField]
        public bool pauseTime { get; set; } = true;
        [field: SerializeField]
        public bool showCursor { get; set; } = true;

        public UnityEvent onPause;

        public UnityEvent onUnPause;

        [HideInInspector]
        public bool paused = false;

        [HideInInspector]
        public bool modifyingTimeScale = false;

        [HideInInspector]
        public float currentTimeDuration = 0;

        [HideInInspector]
        public float currentTimeMultiplier = 0;

        private float initialTimeScale;

        [Header("FPS Counter:")]
        [Tooltip("Should fps counters be updated and fps events invoked?")]
        public bool debugFPS = false;
        public Text fpsCounter;
        public TMP_Text fpsCounterTMP;
        [Min(1)]
        public float targetFPS = 60;
        [HideInInspector]
        public float currentFPS;
        public UnityEvent onBelowTargetFPS;
        public UnityEvent onAboveTargetFPS;

        #endregion

        #region

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            initialTimeScale = Time.timeScale;
        }

        private void Update()
        {
            if(Input.GetKeyDown(togglePauseKey) == true)
            {
                TogglePause();
            }

            if(debugFPS == true)
            {
                CountFPS();
            }
        }

        private IEnumerator ModifyTimeScale()
        {
            Time.timeScale += currentTimeMultiplier;
            modifyingTimeScale = true;
            yield return new WaitForSeconds(currentTimeDuration);
            if (paused == false)
            {
                Time.timeScale = initialTimeScale;
            }
            else
            {
                Time.timeScale = 0;
            }
            currentTimeMultiplier = 0;
            currentTimeDuration = 0;
            modifyingTimeScale = false;
        }

        #endregion

        #region Methods:

        public void TogglePause()
        {
            if (paused == true)
            {
                if (pauseTime == true)
                {
                    Time.timeScale = 0f;
                }

                if (pauseAudio == true)
                {
                    AudioListener.pause = true;
                }

                if(showCursor == true)
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                }
                onPause.Invoke();
            }
            else
            {
                Time.timeScale = initialTimeScale;
                AudioListener.pause = false;
                if(showCursor == true)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                onUnPause.Invoke();
            }
            paused = !paused;
        }

        public void CountFPS()
        {
            currentFPS = 1f / Time.unscaledDeltaTime;

            if(fpsCounterTMP != null)
            {
                fpsCounterTMP.text = "FPS: " + ((int)currentFPS).ToString();
            }
            else
            {
                if (fpsCounter != null)
                {
                    fpsCounter.text = "FPS: " + ((int)currentFPS).ToString();
                }
            }
            
            if(currentFPS < targetFPS)
            {
                onBelowTargetFPS.Invoke();
            }
            else if(currentFPS > targetFPS)
            {
                onAboveTargetFPS.Invoke();
            }
        }

        public void DebugFPS()
        {
            Debug.Log(((int)currentFPS).ToString());
        }  

        /// <summary>
        /// Scales the time of the game for the specified duration.
        /// </summary>
        /// <param name="multiplier">How much to add to the timescale.</param>
        /// <param name="duration">The duration of the effect.</param>
        /// <param name="addMultiplier">Should the current multiplier be added onto.</param>
        /// <param name="addDuration">Should the current durration be added onto.</param>
        public void ScaleTime(float multiplier, float duration, bool addMultiplier, bool addDuration)
        {
            if (modifyingTimeScale == true)
            {
                StopCoroutine(ModifyTimeScale());
                if (addDuration == true)
                {
                    currentTimeDuration += duration;
                }
                else
                {
                    currentTimeDuration = duration;
                }

                if (addMultiplier == true)
                {
                    currentTimeMultiplier += duration;
                }
                else
                {
                    currentTimeMultiplier = multiplier;
                }
                StartCoroutine(ModifyTimeScale());
            }
            else
            {
                currentTimeDuration = duration;
                currentTimeMultiplier = multiplier;
                StartCoroutine(ModifyTimeScale());
            }
        }

        /// <summary>
        /// Slows the time scale to half speed (overrides ScaleTime effects).
        /// </summary>
        /// <param name="duration">How long the slow motion effect lasts.</param>
        public void SlowMoHalfSpeed(float duration)
        {
            if (modifyingTimeScale == true)
            {
                StopCoroutine(ModifyTimeScale());
            }
            currentTimeDuration = duration;
            Time.timeScale = initialTimeScale / 2;
            currentTimeMultiplier = 0;
            StartCoroutine(ModifyTimeScale());
        }

        #endregion
    }
}