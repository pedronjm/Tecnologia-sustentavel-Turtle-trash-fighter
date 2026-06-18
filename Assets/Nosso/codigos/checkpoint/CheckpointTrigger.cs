using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CheckpointTrigger : MonoBehaviour
{
    [Tooltip("ID unico do checkpoint para save remoto.")]
    [SerializeField]
    private string checkpointId;

    [Header("Interacao")]
    [SerializeField]
    private GameObject interactionPrompt;

    [SerializeField]
    private TMP_Text interactionPromptText;

    [TextArea]
    [FormerlySerializedAs("interactionMessage")]
    [SerializeField]
    private string interactionMessageTemplate = "Salvar ponto de respawn? Pressione {0}";

    [Header("Status do Checkpoint")]
    [SerializeField]
    private GameObject selectedCheckpointCanvas;

    [SerializeField]
    private TMP_Text selectedCheckpointText;

    [TextArea]
    [SerializeField]
    private string selectedCheckpointMessage = "Checkpoint ja selecionado";

    private bool playerInRange;
    private bool checkpointSavedThisVisit;
    private bool checkpointAlreadySelected;

    private CheckpointData checkpointData;

    void Awake()
    {
        MenuBindingStore.EnsureLoaded();

        checkpointData = GetComponent<CheckpointData>();

        AutoAssignPromptReferences();
        AutoAssignSelectedReferences();

        HidePrompt();
        HideSelectedCanvas();
    }

    private void OnEnable()
    {
        MenuBindingStore.BindingsChanged += HandleBindingsChanged;
    }

    private void OnDisable()
    {
        MenuBindingStore.BindingsChanged -= HandleBindingsChanged;
    }

    private string GetCheckpointId()
    {
        if (checkpointData != null && !string.IsNullOrEmpty(checkpointData.checkpointId))
        {
            return checkpointData.checkpointId;
        }

        if (!string.IsNullOrEmpty(checkpointId))
            return checkpointId;

        return gameObject.name;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerInRange = true;
        checkpointAlreadySelected = IsCurrentCheckpointSelected();

        Debug.Log($"Checkpoint entrou em contato com: {other.name}");

        if (checkpointAlreadySelected)
            ShowSelectedCanvas();
        else if (!checkpointSavedThisVisit)
            ShowPrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerInRange = false;
        checkpointSavedThisVisit = false;
        Debug.Log($"Checkpoint saiu de contato com: {other.name}");
        HidePrompt();
        HideSelectedCanvas();
    }

    void Update()
    {
        if (!playerInRange || checkpointSavedThisVisit || checkpointAlreadySelected)
            return;

        if (MenuBindingStore.WasPressedThisFrame(MenuActionId.Interact))
            SaveCheckpoint();
    }

    private void ShowPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);

        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(true);

            string interactLabel = MenuBindingStore.GetDisplayName(MenuActionId.Interact);

            if (string.IsNullOrEmpty(interactLabel))
            {
                interactLabel = "Interagir";
            }

            interactionPromptText.text = string.Format(interactionMessageTemplate, interactLabel);

            Debug.Log($"Checkpoint botão atual: {interactLabel}");
        }

        HideSelectedCanvas();
    }

    private void HidePrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (interactionPromptText != null)
            interactionPromptText.gameObject.SetActive(false);
    }

    private void ShowSelectedCanvas()
    {
        if (selectedCheckpointCanvas != null)
            selectedCheckpointCanvas.SetActive(true);

        if (selectedCheckpointText != null)
        {
            selectedCheckpointText.gameObject.SetActive(true);
            selectedCheckpointText.text = selectedCheckpointMessage;
            Debug.Log($"CheckpointTrigger.ShowSelectedCanvas text='{selectedCheckpointMessage}'");
        }

        HidePrompt();
    }

    private void HideSelectedCanvas()
    {
        if (selectedCheckpointCanvas != null)
            selectedCheckpointCanvas.SetActive(false);

        if (selectedCheckpointText != null)
            selectedCheckpointText.gameObject.SetActive(false);
    }

    private void AutoAssignPromptReferences()
    {
        if (interactionPrompt == null)
        {
            Canvas canvas = GetComponentInChildren<Canvas>(true);
            if (canvas != null)
                interactionPrompt = canvas.gameObject;
        }

        if (interactionPromptText == null && interactionPrompt != null)
            interactionPromptText = interactionPrompt.GetComponentInChildren<TMP_Text>(true);

        if (interactionPromptText == null)
            interactionPromptText = GetComponentInChildren<TMP_Text>(true);

        if (interactionPrompt == null && interactionPromptText != null)
            interactionPrompt =
                interactionPromptText.transform.parent != null
                    ? interactionPromptText.transform.parent.gameObject
                    : interactionPromptText.gameObject;
    }

    private void AutoAssignSelectedReferences()
    {
        if (selectedCheckpointCanvas == null)
        {
            Canvas canvas = GetComponentInChildren<Canvas>(true);
            if (canvas != null)
                selectedCheckpointCanvas = canvas.gameObject;
        }

        if (selectedCheckpointText == null && selectedCheckpointCanvas != null)
            selectedCheckpointText = selectedCheckpointCanvas.GetComponentInChildren<TMP_Text>(
                true
            );

        if (selectedCheckpointText == null)
            selectedCheckpointText = GetComponentInChildren<TMP_Text>(true);

        if (selectedCheckpointCanvas == null && selectedCheckpointText != null)
            selectedCheckpointCanvas =
                selectedCheckpointText.transform.parent != null
                    ? selectedCheckpointText.transform.parent.gameObject
                    : selectedCheckpointText.gameObject;
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other == null)
            return false;

        if (other.CompareTag("Player"))
            return true;

        return other.GetComponentInParent<Player>() != null;
    }

    private bool IsCurrentCheckpointSelected()
    {
        if (CheckpointState.instance == null)
            return false;

        return CheckpointState.instance.HasCheckpoint()
            && CheckpointState.instance.CurrentCheckpointId == GetCheckpointId();
    }

    private void SaveCheckpoint()
    {
        if (CheckpointState.instance != null)
        {
            CheckpointState.instance.SetCheckpoint(GetCheckpointId());
        }

        checkpointSavedThisVisit = true;

        checkpointAlreadySelected = true;

        HidePrompt();

        ShowSelectedCanvas();

        Debug.Log("Checkpoint salvo: " + GetCheckpointId());
    }

    private void HandleBindingsChanged()
    {
        if (!playerInRange)
            return;

        if (checkpointAlreadySelected)
        {
            ShowSelectedCanvas();
            return;
        }

        if (!checkpointSavedThisVisit)
            ShowPrompt();
    }
}
