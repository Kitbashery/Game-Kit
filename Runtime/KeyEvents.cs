using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

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

/// <summary>
/// Invokes <see cref="UnityEvent"/>s based on keyboard key input.
/// See Unity manual for details about input system migration:
/// https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Migration.html
/// </summary>
public class KeyEvents : MonoBehaviour
{
    #region Properties:

    public bool allowInput { get; set; } = true;

    public List<KeyEvent> keyEvents;

    [Tooltip("Should any key X events be used?")]
    public bool checkAnyKey = false;
    public UnityEvent onAnyKeyPress;
    [Tooltip("Note: Only implemented for legacy input system.")]
    public UnityEvent onAnyKeyHeld;

    #endregion

    #region Initialization & Updates:

    void Update()
    {
        if(allowInput == true)
        {
            if (keyEvents.Count > 0)
            {
                foreach (KeyEvent input in keyEvents)
                {
                    EvaluateKeyEvent(input);
                }
            }

            if (checkAnyKey == true)
            {
#if (ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER) || ENABLE_INPUT_SYSTEM

                if (Keyboard.current.anyKey.isPressed == true)
                {
                    onAnyKeyPress.Invoke();
                }

                /* if (Keyboard.current.anyKey.wasUpdatedThisFrame)
                 {
                     onAnyKeyPress.Invoke();
                 }*/
#else

        if(Input.anyKey == true)
        {
            onAnyKeyPress.Invoke();
        }

        if(Input.anyKeyDown == true)
        {
            onAnyKeyHeld.Invoke();
        }

#endif
            }
        }
    }

    #endregion

    #region Methods:

    public void EvaluateKeyEvent(KeyEvent input)
    {

#if (ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER) || ENABLE_INPUT_SYSTEM

        if (IsKeyTriggered(input.key, input.trigger) == true)
        {
            input.action.Invoke();
        }
#else

#if ENABLE_LEGACY_INPUT_MANAGER

                    if (IsKeyTriggered(input.legacyKey, input.trigger) == true)
                    {
                        input.uEvent.Invoke();
                    }
#endif

#endif

    }

#if ENABLE_INPUT_SYSTEM

    public bool IsKeyTriggered(Key key, InputTrigger trigger)
    {
        switch(trigger)
        {
            case InputTrigger.WhenDown:

                return (Keyboard.current[key]).wasPressedThisFrame;

            case InputTrigger.WhenUp:

                return (Keyboard.current[key]).wasReleasedThisFrame;

            default:

                return false;
        }
    }

#endif

#if ENABLE_LEGACY_INPUT_MANAGER

    public bool IsKeyTriggered(Key key, InputTrigger trigger)
    {
        switch(trigger)
        {
            case InputTrigger.WhenDown:

                 return Input.GetKeyDown(key) == true;

            case InputTrigger.WhenUp:

                return Input.GetKeyUp(key) == true;

            default:

                return false;
        }
    }

#endif

    #endregion

}

[Serializable]
public struct KeyEvent
{
    [Tooltip("The key to use if using the 2021+ input system.")]
#if ENABLE_INPUT_SYSTEM
    public Key key;
#endif
    [Tooltip("The key to use if using the legacy input system.")]
    public KeyCode legacyKey;
    public InputTrigger trigger;
    public UnityEvent action;
}

public enum InputTrigger { WhenDown, WhenUp }
