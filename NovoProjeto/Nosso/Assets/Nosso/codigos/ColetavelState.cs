using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Guarda em memória quais coletáveis já foram coletados (por ID).
/// Persiste entre carregamento de cenas (DontDestroyOnLoad).
/// Não grava em disco — quando quiser salvar/carregar, use GetCollectedIds() e CarregarIds().
/// </summary>
public class ColetavelState : MonoBehaviour
{
    public static ColetavelState instance { get; private set; }

    private HashSet<string> collectedIds = new HashSet<string>();

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Registra que o coletável com este ID foi coletado.</summary>
    public void RegistrarColetado(string id)
    {
        if (string.IsNullOrEmpty(id))
            return;
        collectedIds.Add(id);
    }

    /// <summary>Retorna true se este ID já foi coletado.</summary>
    public bool FoiColetado(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;
        return collectedIds.Contains(id);
    }

    /// <summary>Para quando você implementar save: retorna todos os IDs já coletados (ex.: para gravar em JSON/PlayerPrefs).</summary>
    public IReadOnlyCollection<string> GetCollectedIds()
    {
        return collectedIds;
    }

    /// <summary>Para quando você implementar load: restaura a lista de IDs coletados (ex.: após ler do disco).</summary>
    public void CarregarIds(IEnumerable<string> ids)
    {
        collectedIds.Clear();
        if (ids != null)
        {
            foreach (string id in ids)
                if (!string.IsNullOrEmpty(id))
                    collectedIds.Add(id);
        }
    }

    /// <summary>Limpa o estado (útil para novo jogo).</summary>
    public void Limpar()
    {
        collectedIds.Clear();
    }
}
