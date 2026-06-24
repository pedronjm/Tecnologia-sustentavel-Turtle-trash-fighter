using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseSaveButton : MonoBehaviour
{
    [SerializeField]
    private Button saveButton;

    [SerializeField]
    private TMP_Text feedbackLabel; // opcional

    [SerializeField]
    private int slotIndex = 0; // Índice do slot de salvamento

    private void OnEnable()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveClicked);
    }

    private void OnDisable()
    {
        if (saveButton != null)
            saveButton.onClick.RemoveListener(OnSaveClicked);
    }

    private void OnSaveClicked()
    {
        var service = RemoteSaveService.getInstance();

        if (service == null)
        {
            Debug.LogError("RemoteSaveService não encontrado!");
            return;
        }
        getSlotIndex();
        service.SaveGame(slotIndex);

        if (feedbackLabel != null)
            feedbackLabel.text = "Jogo salvo!";

        Debug.Log("Save solicitado pelo pause.");
    }

    private void getSlotIndex()
    {
        if (CurrentSaveSession.instance != null)
        {
            Debug.Log(
                "Slot salvo na sessão antes da conversão: "
                    + CurrentSaveSession.instance.SelectedSlot
            );

            slotIndex = CurrentSaveSession.instance.SelectedSlot + 1;

            Debug.Log("Slot enviado para API: " + slotIndex);
        }
        else
        {
            Debug.LogError("CurrentSaveSession não encontrada!");
        }
    }
}
