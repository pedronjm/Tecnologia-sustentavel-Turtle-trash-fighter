using System;
using UnityEngine;

public class SelectedCharacterActivator : MonoBehaviour
{
    [Serializable]
    private struct CharacterObject
    {
        public PlayableCharacterId id;
        public GameObject rootObject;
    }

    [SerializeField] private CharacterObject[] characterObjects;

    private void Start()
    {
        ApplySelection();
    }

    public void ApplySelection()
    {
        if (characterObjects == null || characterObjects.Length == 0)
            return;

        PlayableCharacterId selected = NewGameSessionSettings.SelectedCharacter;
        int selectedIndex = -1;

        for (int i = 0; i < characterObjects.Length; i++)
        {
            if (characterObjects[i].id == selected)
            {
                selectedIndex = i;
                break;
            }
        }

        if (selectedIndex < 0)
            selectedIndex = 0;

        for (int i = 0; i < characterObjects.Length; i++)
        {
            GameObject character = characterObjects[i].rootObject;
            if (character != null)
                character.SetActive(i == selectedIndex);
        }
    }
}
