using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/FadeEventSO")]
public class FadeEventSO : ScriptableObject
{
    public UnityAction<Color,float,bool> OnEventRaised;
    
    /// <summary>
    /// turn to black slowly
    /// </summary>
    /// <param name="duration">time</param>
    public void FadeIn(float duration)
    {
        RaiseEvent(Color.black, duration,true);
    }

    /// <summary>
    /// turn to transparent
    /// </summary>
    /// <param name="duration"></param>
    public void FadeOut(float duration)
    {
        RaiseEvent(Color.clear, duration,false);
    }

    public void RaiseEvent(Color target, float duration, bool fadeIn)
    {
        OnEventRaised?.Invoke(target,duration,fadeIn);
    }
    
}
