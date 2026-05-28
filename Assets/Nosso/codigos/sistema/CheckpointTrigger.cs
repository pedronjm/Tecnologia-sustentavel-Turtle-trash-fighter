using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheckpointTrigger : MonoBehaviour
{
    [Tooltip("ID unico do checkpoint para save remoto.")]
    [SerializeField] private string checkpointId;

    [Header("Interacao")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TMP_Text interactionPromptText;
    [TextArea]
    [SerializeField] private string interactionMessage = "Salvar ponto de respawn? Pressione E";

    [Header("Status do Checkpoint")]
    [SerializeField] private GameObject selectedCheckpointCanvas;
    [SerializeField] private TMP_Text selectedCheckpointText;
    [TextArea]
    [SerializeField] private string selectedCheckpointMessage = "Checkpoint ja selecionado";

    private bool playerInRange;
    private bool checkpointSavedThisVisit;
    private bool checkpointAlreadySelected;

    void Awake()
    {
        AutoAssignPromptReferences();
        AutoAssignSelectedReferences();
        HidePrompt();
        HideSelectedCanvas();
    }

    private string GetCheckpointId()
    {
        if (!string.IsNullOrEmpty(checkpointId))
            return checkpointId;

        var p = transform.position;
        return $"{gameObject.scene.name}:{gameObject.name}:{Mathf.RoundToInt(p.x * 100f)}:{Mathf.RoundToInt(p.y * 100f)}";
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

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            SaveCheckpoint();
    }

    private void ShowPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);

        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(true);
            interactionPromptText.text = interactionMessage;
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
        if (interactionPromptText == null)
            interactionPromptText = GetComponentInChildren<TMP_Text>(true);

        if (interactionPrompt == null)
        {
            if (interactionPromptText != null)
            {
                interactionPrompt = interactionPromptText.transform.parent != null
                    ? interactionPromptText.transform.parent.gameObject
                    : interactionPromptText.gameObject;
            }
            else
            {
                Canvas canvas = GetComponentInChildren<Canvas>(true);
                if (canvas != null)
                    interactionPrompt = canvas.gameObject;
            }
        }

        if (interactionPrompt == null && interactionPromptText != null)
            interactionPrompt = interactionPromptText.gameObject;
    }

    private void AutoAssignSelectedReferences()
    {
        if (selectedCheckpointText == null)
            selectedCheckpointText = GetComponentInChildren<TMP_Text>(true);

        if (selectedCheckpointCanvas == null)
        {
            if (selectedCheckpointText != null)
            {
                selectedCheckpointCanvas = selectedCheckpointText.transform.parent != null
                    ? selectedCheckpointText.transform.parent.gameObject
                    : selectedCheckpointText.gameObject;
            }
        }

        if (selectedCheckpointCanvas == null && selectedCheckpointText != null)
            selectedCheckpointCanvas = selectedCheckpointText.gameObject;
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

        return CheckpointState.instance.HasCheckpoint() &&
               CheckpointState.instance.CurrentCheckpointId == GetCheckpointId();
    }

    private void SaveCheckpoint()
    {
        if (GameControler.instance != null)
        {
            GameControler.instance.SetCheckpoint(transform.position, GetCheckpointId());
        }
        else if (CheckpointState.instance != null)
        {
            CheckpointState.instance.SetCheckpoint(GetCheckpointId(), transform.position);
        }

        checkpointSavedThisVisit = true;
        checkpointAlreadySelected = true;
        HidePrompt();
        ShowSelectedCanvas();

        if (CheckpointState.instance != null)
            Debug.Log($"Checkpoint ativado: {CheckpointState.instance.CurrentCheckpointId}");
    }
}