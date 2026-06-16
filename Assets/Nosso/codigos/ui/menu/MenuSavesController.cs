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

    private void Start()
    {
        AtualizarSlots();

        loadButton.onClick.AddListener(CarregarSave);
        deleteButton.onClick.AddListener(DeletarSave);

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i;

            slotButtons[i]
                .onClick.AddListener(() =>
                {
                    SelecionarSlot(index);
                });
        }

        AtualizarBotoes();
    }

    private void AtualizarSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            SaveSlot slot = SaveSlotManager.LoadSlot(i);

            if (slot.hasData)
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
                    + "\n\nClique para selecionar";
            }
            else
            {
                slotTexts[i].text = "SLOT " + (i + 1) + "\n\nNOVO JOGO";
            }
        }
    }

    private void SelecionarSlot(int index)
    {
        selectedSlot = index;

        SaveSlot slot = SaveSlotManager.LoadSlot(index);

        if (!slot.hasData)
        {
            MenuNewGameFlowController flow = FindFirstObjectByType<MenuNewGameFlowController>();

            if (flow != null)
            {
                flow.SetSelectedSlot(index);
            }

            // abre tela de novo jogo
            MenuUIController menu = FindFirstObjectByType<MenuUIController>();

            if (menu != null)
            {
                menu.ShowNewGameOptions();
            }

            return;
        }

        AtualizarBotoes();
    }

    private void CriarNovoSave()
    {
        SaveSlotManager.CreateSaveFromCurrent(selectedSlot);

        AtualizarSlots();

        Debug.Log("Novo save criado no Slot " + (selectedSlot + 1));
    }

    private void CarregarSave()
    {
        if (selectedSlot < 0)
            return;

        SaveSlot slot = SaveSlotManager.LoadSlot(selectedSlot);

        if (!slot.hasData)
            return;

        SaveSlotManager.LoadGameFromSlot(selectedSlot);

        SceneManager.LoadScene("SampleScene");
    }

    private void DeletarSave()
    {
        if (selectedSlot < 0)
            return;

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

        loadButton.interactable = temSaveSelecionado;

        deleteButton.interactable = temSaveSelecionado;
    }
}
