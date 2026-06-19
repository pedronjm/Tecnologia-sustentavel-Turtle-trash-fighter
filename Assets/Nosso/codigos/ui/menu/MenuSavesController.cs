using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSavesController : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private Button[] slotButtons;
    [SerializeField] private TMP_Text[] slotTexts;

    [Header("Botoes")]
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;

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

    private void AtualizarSlots()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            SaveSlot slot = SaveSlotManager.LoadSlot(i);

            // Destaca visualmente se este slot for o selecionado atual
            string sufixoSelecao = (i == selectedSlot) ? "\n<color=yellow><b>[ SELECIONADO ]</b></color>" : "\nClique para selecionar";

            if (slot != null && slot.hasData)
            {
                slotTexts[i].text =
                    "SLOT " + (i + 1) + "\n\n" +
                    "Personagem: " + slot.selectedCharacter + "\n" +
                    "Dificuldade: " + slot.difficulty + "\n" +
                    "Progresso: " + slot.completionPercent.ToString("F1") + "%" +
                    sufixoSelecao;
            }
            else
            {
                slotTexts[i].text = "SLOT " + (i + 1) + "\n\nNOVO JOGO";
            }
        }
    }

    private void SelecionarSlot(int index)
    {
        // Se o jogador clicar no slot que já estava selecionado, podemos carregar direto! (Opcional)
        if (selectedSlot == index)
        {
            SaveSlot slotExistente = SaveSlotManager.LoadSlot(index);
            if (slotExistente != null && slotExistente.hasData)
            {
                CarregarSave();
                return;
            }
        }

        selectedSlot = index;
        SaveSlot slot = SaveSlotManager.LoadSlot(index);

        // Se o slot estiver vazio, inicia o fluxo de Novo Jogo
        if (slot == null || !slot.hasData)
        {
            MenuNewGameFlowController flow = FindFirstObjectByType<MenuNewGameFlowController>();
            if (flow != null) flow.SetSelectedSlot(index);

            MenuUIController menu = FindFirstObjectByType<MenuUIController>();
            if (menu != null) menu.ShowNewGameOptions();

            // Reseta seleção já que abriu outra tela
            selectedSlot = -1; 
            AtualizarBotoes();
            return;
        }

        // Se tem dados, atualiza o texto dos slots (para mostrar o aviso de "SELECIONADO") e os botões
        AtualizarSlots();
        AtualizarBotoes();
    }

    private void CarregarSave()
    {
        if (selectedSlot < 0) return;

        SaveSlot slot = SaveSlotManager.LoadSlot(selectedSlot);
        if (slot == null || !slot.hasData) return;

        Debug.Log($"Carregando Slot {selectedSlot + 1}...");
        SaveSlotManager.LoadGameFromSlot(selectedSlot);

        // Certifique-se de que "SampleScene" é o nome exato da sua cena de jogo
        SceneManager.LoadScene("SampleScene");
    }

    private void DeletarSave()
    {
        if (selectedSlot < 0) return;

        SaveSlotManager.DeleteSlot(selectedSlot);
        selectedSlot = -1;

        AtualizarSlots();
        AtualizarBotoes();
    }

    private void AtualizarBotoes()
    {
        bool temSaveSelecionado = false;

        if (selectedSlot >= 0)
        {
            SaveSlot slot = SaveSlotManager.LoadSlot(selectedSlot);
            temSaveSelecionado = slot != null && slot.hasData;
        }

        // Ativa/Desativa os botões na UI dependendo se há um save válido selecionado
        if (loadButton != null) loadButton.interactable = temSaveSelecionado;
        if (deleteButton != null) deleteButton.interactable = temSaveSelecionado;
    }
}