using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSavesController : MonoBehaviour
{
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private TMP_Text selectedSlotLabel;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private TMP_Text statusLabel;

    private int selectedSlotIndex = -1;
    private List<SaveSlotUI> slotUIs = new List<SaveSlotUI>();

    private void OnEnable()
    {
        RefreshSlots();
        WireButtons();
        SaveSlotManager.SavesChanged += RefreshSlots;
    }

    private void OnDisable()
    {
        SaveSlotManager.SavesChanged -= RefreshSlots;
    }

    public void RefreshSlots()
    {
        // Limpa UI antiga
        foreach (Transform child in slotsContainer)
            Destroy(child.gameObject);

        slotUIs.Clear();

        // Cria nova UI
        List<SaveSlot> slots = SaveSlotManager.GetAllSlots();
        for (int i = 0; i < slots.Count; i++)
        {
            CreateSlotUI(slots[i]);
        }

        selectedSlotIndex = -1;
        UpdateButtonStates();
    }

    private void CreateSlotUI(SaveSlot slot)
    {
        GameObject slotGO = Instantiate(slotPrefab, slotsContainer);
        SaveSlotUI slotUI = slotGO.GetComponent<SaveSlotUI>();

        if (slotUI == null)
            slotUI = slotGO.AddComponent<SaveSlotUI>();

        slotUI.Initialize(slot, OnSlotSelected);
        slotUIs.Add(slotUI);
    }

    private void OnSlotSelected(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        UpdateSelectedSlotUI();
        UpdateButtonStates();
    }

    private void UpdateSelectedSlotUI()
    {
        foreach (SaveSlotUI slotUI in slotUIs)
            slotUI.SetSelected(slotUI.SlotIndex == selectedSlotIndex);

        if (selectedSlotIndex >= 0 && selectedSlotIndex < slotUIs.Count)
        {
            SaveSlot slot = SaveSlotManager.LoadSlot(selectedSlotIndex);
            string slotInfo = $"{slot.slotName}\n";

            if (slot.hasData)
                slotInfo += $"Progresso: {slot.completionPercent:F1}%\nSalvo em: {slot.lastSavedTime}";
            else
                slotInfo += "Vazio";

            if (selectedSlotLabel != null)
                selectedSlotLabel.text = slotInfo;
        }
    }

    private void UpdateButtonStates()
    {
        SaveSlot slot = selectedSlotIndex >= 0 ? SaveSlotManager.LoadSlot(selectedSlotIndex) : null;

        if (loadButton != null)
            loadButton.interactable = slot != null && slot.hasData;

        if (saveButton != null)
            saveButton.interactable = selectedSlotIndex >= 0;

        if (deleteButton != null)
            deleteButton.interactable = slot != null && slot.hasData;
    }

    public void OnLoadButtonClicked()
    {
        if (selectedSlotIndex < 0)
            return;

        SaveSlot slot = SaveSlotManager.LoadSlot(selectedSlotIndex);
        if (!slot.hasData)
            return;

        SaveSlotManager.LoadGameFromSlot(selectedSlotIndex);
        SetStatus($"Carregado: {slot.slotName}");

        // Carrega a cena do jogo
        SceneManager.LoadScene("SampleScene");
    }

    public void OnSaveButtonClicked()
    {
        if (selectedSlotIndex < 0)
            return;

        SaveSlot newSlot = SaveSlotManager.CreateSaveFromCurrent(selectedSlotIndex);
        SetStatus($"Salvo em: {newSlot.slotName}");
        RefreshSlots();
    }

    public void OnDeleteButtonClicked()
    {
        if (selectedSlotIndex < 0)
            return;

        SaveSlot slot = SaveSlotManager.LoadSlot(selectedSlotIndex);
        SaveSlotManager.DeleteSlot(selectedSlotIndex);
        SetStatus($"Deletado: {slot.slotName}");
        RefreshSlots();
    }

    private void WireButtons()
    {
        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadButtonClicked);

        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveButtonClicked);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
    }

    private void SetStatus(string message)
    {
        if (statusLabel != null)
            statusLabel.text = message;
    }
}

public class SaveSlotUI : MonoBehaviour
{
    private int slotIndex;
    private SaveSlot saveSlot;
    private Button selectButton;
    private TMP_Text slotLabel;
    private Image slotBg;
    private Color defaultColor;
    private Color selectedColor = new Color(0.2f, 0.6f, 1f, 0.8f);

    public int SlotIndex => slotIndex;

    public void Initialize(SaveSlot slot, System.Action<int> onSelected)
    {
        slotIndex = slot.slotIndex;
        saveSlot = slot;

        selectButton = GetComponent<Button>();
        slotLabel = GetComponentInChildren<TMP_Text>();
        slotBg = GetComponent<Image>();

        if (slotBg != null)
            defaultColor = slotBg.color;

        RefreshUI();

        if (selectButton != null)
            selectButton.onClick.AddListener(() => onSelected?.Invoke(slotIndex));
    }

    public void SetSelected(bool isSelected)
    {
        if (slotBg == null)
            return;

        slotBg.color = isSelected ? selectedColor : defaultColor;
    }

    private void RefreshUI()
    {
        if (slotLabel == null)
            return;

        string labelText = saveSlot.slotName;

        if (saveSlot.hasData)
            labelText += $"\n{saveSlot.completionPercent:F1}%\n{saveSlot.lastSavedTime}";
        else
            labelText += "\nVazio";

        slotLabel.text = labelText;
    }
}
