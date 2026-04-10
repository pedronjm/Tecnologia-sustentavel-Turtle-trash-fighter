using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Referências")]
    public Damageable damageable;
    public Image fillImage;
    public Transform lookAtCamera;

    private void Reset()
    {
        BindReferences();
    }

    private void Awake()
    {
        BindReferences();
        ConfigureCanvas();
    }

    private void OnEnable()
    {
        if (damageable != null)
        {
            damageable.OnHealthChanged.AddListener(UpdateBar);
        }

        RefreshBar();
    }

    private void Start()
    {
        ConfigureCanvas();
        RefreshBar();
    }

    private void OnDisable()
    {
        if (damageable != null)
        {
            damageable.OnHealthChanged.RemoveListener(UpdateBar);
        }
    }

    private void LateUpdate()
    {
        if (lookAtCamera == null && Camera.main != null)
        {
            lookAtCamera = Camera.main.transform;
        }

        if (lookAtCamera != null)
        {
            transform.forward = lookAtCamera.forward;
        }
    }

    private void UpdateBar(float value01)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(value01);
        }
    }

    private void RefreshBar()
    {
        if (damageable == null)
        {
            return;
        }

        float value01 = 0f;
        if (damageable.maxHealth > 0)
        {
            value01 = (float)damageable.currentHealth / damageable.maxHealth;
        }

        UpdateBar(value01);
    }

    private void ConfigureCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            return;
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        if (Camera.main != null)
        {
            canvas.worldCamera = Camera.main;
        }
    }

    private void BindReferences()
    {
        if (damageable == null)
        {
            damageable = GetComponentInParent<Damageable>();
        }

        if (fillImage == null)
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                Image candidate = images[i];
                if (candidate == null)
                    continue;

                string candidateName = candidate.name.ToLowerInvariant();
                bool looksLikeFill = candidateName.Contains("fill");
                if (!looksLikeFill)
                    looksLikeFill = candidateName.Contains("barfill");
                if (!looksLikeFill)
                    looksLikeFill = candidate.type == Image.Type.Filled;

                if (looksLikeFill)
                {
                    fillImage = candidate;
                    break;
                }

                if (fillImage == null)
                    fillImage = candidate;
            }
        }

        if (lookAtCamera == null && Camera.main != null)
        {
            lookAtCamera = Camera.main.transform;
        }
    }

    private void OnValidate()
    {
        BindReferences();
        ConfigureCanvas();
        RefreshBar();
    }
}
