using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/CharacterEventSO")]
public class CharacterEventSO : ScriptableObject
{
    
    public UnityAction<Character> OnEventRaised;

    /// <summary>
    /// このイベントを引用する
    /// </summary>
    /// <param name="character">引用するcharacter</param>
    public void RaiseEvent(Character character)
    {
        OnEventRaised?.Invoke(character);
    }
    
}
