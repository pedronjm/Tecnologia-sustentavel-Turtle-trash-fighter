using UnityEngine;
using UnityEngine.UI;

public class HudColeveis : MonoBehaviour
{
    public enum OrigemContador
    {
        Garrafa,
        Engrenagem,
        Maca,
        Circuito,
        TotalColetados,
    }

    [System.Serializable]
    public class CampoTextoLixo
    {
        public string prefixo = "Lixo: ";
        public OrigemContador origem = OrigemContador.TotalColetados;
        public Text textoUI;
        public TMPro.TMP_Text textoTMP;
    }

    [Header("Referencias")]
    [Tooltip("Se vazio, tenta usar GameControler.instance automaticamente.")]
    public GameControler gameControler;

    [Header("5 textos para os lixos")]
    public CampoTextoLixo lixo1 = new CampoTextoLixo
    {
        prefixo = "Garrafa: ",
        origem = OrigemContador.Garrafa,
    };

    public CampoTextoLixo lixo2 = new CampoTextoLixo
    {
        prefixo = "Engrenagem: ",
        origem = OrigemContador.Engrenagem,
    };

    public CampoTextoLixo lixo3 = new CampoTextoLixo
    {
        prefixo = "Maca: ",
        origem = OrigemContador.Maca,
    };

    public CampoTextoLixo lixo4 = new CampoTextoLixo
    {
        prefixo = "Circuito: ",
        origem = OrigemContador.Circuito,
    };

    public CampoTextoLixo lixo5 = new CampoTextoLixo
    {
        prefixo = "Total: ",
        origem = OrigemContador.TotalColetados,
    };

    private int[] ultimoValor = new int[] { -1, -1, -1, -1, -1 };

    void Awake()
    {
        TentarEncontrarGameControler();
    }

    void Start()
    {
        AtualizarHUD(true);
    }

    void Update()
    {
        AtualizarHUD();
    }

    void TentarEncontrarGameControler()
    {
        if (gameControler == null)
            gameControler = GameControler.instance;

        if (gameControler == null)
            gameControler = FindFirstObjectByType<GameControler>();
    }

    void AtualizarHUD(bool forcar = false)
    {
        if (gameControler == null)
            TentarEncontrarGameControler();

        if (gameControler == null)
            return;

        AtualizarCampo(lixo1, 0, forcar);
        AtualizarCampo(lixo2, 1, forcar);
        AtualizarCampo(lixo3, 2, forcar);
        AtualizarCampo(lixo4, 3, forcar);
        AtualizarCampo(lixo5, 4, forcar);
    }

    void AtualizarCampo(CampoTextoLixo campo, int indice, bool forcar)
    {
        if (campo == null)
            return;

        int valor = LerValor(campo.origem);
        if (!forcar && valor == ultimoValor[indice])
            return;

        ultimoValor[indice] = valor;
        string textoFinal = campo.prefixo + valor;

        if (campo.textoUI != null)
            campo.textoUI.text = textoFinal;

        if (campo.textoTMP != null)
            campo.textoTMP.text = textoFinal;
    }

    int LerValor(OrigemContador origem)
    {
        switch (origem)
        {
            case OrigemContador.Garrafa:
                return Mathf.RoundToInt(gameControler.qttgarrafa);
            case OrigemContador.Engrenagem:
                return Mathf.RoundToInt(gameControler.qttengrenagem);
            case OrigemContador.Maca:
                return Mathf.RoundToInt(gameControler.qttmaca);
            case OrigemContador.Circuito:
                return Mathf.RoundToInt(gameControler.qttcircuito);
            case OrigemContador.TotalColetados:
                return gameControler.Coletados;
            default:
                return 0;
        }
    }
}
