using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveSlot
{
    public int slotIndex;
    public string slotName;
    public string checkpointId;
    public Vector3 checkpointPosition;
    public float completionPercent;
    public string lastSavedTime;
    public bool hasData;

    public SaveSlot(int index)
    {
        slotIndex = index;
        slotName = $"Slot {index + 1}";
        checkpointId = string.Empty;
        checkpointPosition = Vector3.zero;
        completionPercent = 0f;
        lastSavedTime = string.Empty;
        hasData = false;
    }
}

public static class SaveSlotManager
{
    private const int MaxSlots = 5;
    private const string SaveSlotPrefix = "TurtleTrashFighter.SaveSlot.";

    public static event Action SavesChanged;

    public static int GetMaxSlots() => MaxSlots;

    public static void SaveSlot(int slotIndex, SaveSlot slot)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots)
            return;

        string key = SaveSlotPrefix + slotIndex;
        string json = JsonUtility.ToJson(slot);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
        SavesChanged?.Invoke();
    }

    public static SaveSlot LoadSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots)
            return new SaveSlot(slotIndex);

        string key = SaveSlotPrefix + slotIndex;
        string json = PlayerPrefs.GetString(key, string.Empty);

        if (string.IsNullOrEmpty(json))
            return new SaveSlot(slotIndex);

        SaveSlot slot = JsonUtility.FromJson<SaveSlot>(json);
        if (slot == null)
            return new SaveSlot(slotIndex);

        return slot;
    }

    public static SaveSlot CreateSaveFromCurrent(int slotIndex, string slotName = "")
    {
        SaveSlot slot = new SaveSlot(slotIndex);

        if (!string.IsNullOrEmpty(slotName))
            slot.slotName = slotName;

        if (CheckpointState.instance != null)
        {
            slot.checkpointId = CheckpointState.instance.CurrentCheckpointId;
            slot.checkpointPosition = CheckpointState.instance.LastCheckpointPosition;
            slot.hasData = CheckpointState.instance.HasCheckpoint();
        }

        // Calcula progresso
        float collectibleRatio = 0f;
        float enemyRatio = 0f;

        if (ColetavelState.instance != null && ColetavelState.instance.TotalNaCena > 0)
            collectibleRatio = (float)ColetavelState.instance.ColetadosNaCena / ColetavelState.instance.TotalNaCena;

        if (EnemyState.instance != null && EnemyState.instance.TotalEnemies > 0)
            enemyRatio = (float)EnemyState.instance.DeadEnemies / EnemyState.instance.TotalEnemies;

        float checkpointRatio = slot.hasData ? 1f : 0f;
        slot.completionPercent = (collectibleRatio * 0.5f + enemyRatio * 0.3f + checkpointRatio * 0.2f) * 100f;
        slot.lastSavedTime = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        SaveSlot(slotIndex, slot);
        return slot;
    }

    public static void DeleteSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots)
            return;

        string key = SaveSlotPrefix + slotIndex;
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
        SavesChanged?.Invoke();
    }

    public static List<SaveSlot> GetAllSlots()
    {
        List<SaveSlot> slots = new List<SaveSlot>();

        for (int i = 0; i < MaxSlots; i++)
        {
            slots.Add(LoadSlot(i));
        }

        return slots;
    }

    public static void LoadGameFromSlot(int slotIndex)
    {
        SaveSlot slot = LoadSlot(slotIndex);

        if (!slot.hasData)
        {
            Debug.LogWarning("Slot não possui dados salvos.");
            return;
        }

        if (CheckpointState.instance != null)
            CheckpointState.instance.Restaurar(slot.checkpointId, slot.checkpointPosition);

        if (ColetavelState.instance != null)
            ColetavelState.instance.CarregarIds(new List<string>());

        if (EnemyState.instance != null)
            EnemyState.instance.CarregarInimigosMortos(new List<string>());

        Debug.Log($"Jogo carregado do slot {slot.slotName}");
    }
}
