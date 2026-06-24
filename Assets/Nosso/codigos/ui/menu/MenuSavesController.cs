using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSavesController : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField]
    private Button[] slotButtons;

    [SerializeField]
    private TMP_Text[] slotTexts;

    [Header("Botoes")]
    [SerializeField]
    private Button loadButton;

    [SerializeField]
    private Button deleteButton;

    private int selectedSlot = -1;

    private void OnEnable()
    {
        loadButton.onClick.AddListener(CarregarSave);
        deleteButton.onClick.AddListener(DeletarSave);

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i;
            slotButtons[i].onClick.AddListener(() => SelecionarSlot(index));
        }

        // Reseta a seleção ao abrir a tela
        selectedSlot = -1;
        AtualizarBotoes();

        var service = RemoteSaveService.getInstance();
        if (service != null)
            StartCoroutine(service.CarregarTodosSlots(OnSavesCarregados));
        else
            AtualizarSlots();
    }

    private void OnDisable()
    {
        loadButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        for (int i = 0; i < slotButtons.Length; i++)
            slotButtons[i].onClick.RemoveAllListeners();
    }

    private void OnSavesCarregados()
    {
        AtualizarSlots();
    }

    // Busca o slot no cache em memória (preenchido por RemoteSaveService.CarregarTodosSlots)
    private RemoteSaveService.SaveSlotInfo GetSlotInfo(int index)
    {
        var cache = RemoteSaveService.SlotsCache;
        if (cache == null)
            return null;
        return cache.Find(s => s.slotIndex == index);
    }

    private void AtualizarSlots()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            var slot = GetSlotInfo(i);

            // Destaca visualmente se este slot for o selecionado atual
            string sufixoSelecao =
                (i == selectedSlot)
                    ? "\n<color=yellow><b>[ SELECIONADO ]</b></color>"
                    : "\nClique para selecionar";

            if (slot != null && slot.hasData)
            {
                slotTexts[i].text =
                    "SLOT "
                    + (i + 1)
                    + "\n\n"
                    + "Personagem: "
                    + slot.selectedCharacter
                    + "\n"
                    + "Dificuldade: "
                    + slot.difficulty
                    + "\n"
                    + "Progresso: "
                    + slot.completionPercent.ToString("F1")
                    + "%"
                    + sufixoSelecao;
            }
            else
            {
                slotTexts[i].text = "SLOT " + (i + 1) + "\n\nNOVO JOGO";
            }
        }
    }

    private void SelecionarSlot(int index)
    {
        Debug.Log("Clique no slot: " + index);

        var slot = GetSlotInfo(index);

        if (slot == null)
        {
            Debug.LogError("Slot não existe no cache");
            return;
        }

        Debug.Log("Slot encontrado: " + slot.slotIndex + " hasData=" + slot.hasData);

        selectedSlot = index;

        if (slot.hasData)
        {
            if (CurrentSaveSession.instance != null)
            {
                CurrentSaveSession.instance.SetSlot(index);
            }

            AtualizarSlots();
            AtualizarBotoes();
            return;
        }

        Debug.Log("Slot vazio, criando novo jogo");

        MenuNewGameFlowController flow = FindFirstObjectByType<MenuNewGameFlowController>();

        if (flow != null)
            flow.SetSelectedSlot(index);

        MenuUIController menu = FindFirstObjectByType<MenuUIController>();

        if (menu != null)
            menu.ShowNewGameOptions();
    }

    private void CarregarSave()
    {
        if (selectedSlot < 0)
            return;

        var slot = GetSlotInfo(selectedSlot);
        if (slot == null || !slot.hasData)
            return;

        Debug.Log($"Carregando Slot {selectedSlot + 1}...");

        var service = RemoteSaveService.getInstance();
        if (service != null)
        {
            // Backend usa slotIndex 1-based
            service.LoadGame(selectedSlot + 1);
        }

        // Certifique-se de que "SampleScene" é o nome exato da sua cena de jogo
        SceneManager.LoadScene("SampleScene");
    }

    private void DeletarSave()
    {
        if (selectedSlot < 0)
            return;

        var service = RemoteSaveService.getInstance();
        if (service == null)
            return;

        int slotParaApagar = selectedSlot + 1; // backend é 1-based

        StartCoroutine(
            service.DeleteSlotRoutine(
                slotParaApagar,
                () =>
                {
                    selectedSlot = -1;
                    StartCoroutine(
                        service.CarregarTodosSlots(() =>
                        {
                            AtualizarSlots();
                            AtualizarBotoes();
                        })
                    );
                }
            )
        );
    }

    private void AtualizarBotoes()
    {
        bool temSaveSelecionado = false;

        if (selectedSlot >= 0)
        {
            var slot = GetSlotInfo(selectedSlot);
            temSaveSelecionado = slot != null && slot.hasData;
        }

        // Ativa/Desativa os botões na UI dependendo se há um save válido selecionado
        if (loadButton != null)
            loadButton.interactable = temSaveSelecionado;
        if (deleteButton != null)
            deleteButton.interactable = temSaveSelecionado;
    }
}
