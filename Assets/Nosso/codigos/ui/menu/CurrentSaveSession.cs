using UnityEngine;

public class CurrentSaveSession : MonoBehaviour
{
    public static CurrentSaveSession instance;

    public int SelectedSlot { get; private set; } = -1;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSlot(int slot)
    {
        SelectedSlot = slot;
        Debug.Log("Slot atual salvo na sessão: " + slot);
    }

    public void ClearSlot()
    {
        SelectedSlot = -1;
    }
}
