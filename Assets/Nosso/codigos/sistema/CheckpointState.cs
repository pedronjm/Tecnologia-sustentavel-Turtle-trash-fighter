using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Guarda em memória qual checkpoint está ativo (CurrentCheckpointId) e a
/// posição de cada checkpoint conhecido, indexada por ID.
///
/// IMPORTANTE (correção do bug de respawn): a lista de posições NÃO é mais
/// preenchida manualmente no Inspector. Antes, "checkpoints" era uma lista
/// serializada arrastada à mão em uma única cena; como este objeto usa
/// DontDestroyOnLoad, ele mantinha para sempre a lista da cena onde foi
/// criado primeiro, e checkpoints de outras fases nunca eram encontrados
/// (GetCheckpointPosition() caía no Vector3.zero do fim do método).
///
/// Agora cada CheckpointTrigger se registra aqui sozinho, ao acordar
/// (RegisterCheckpoint), informando seu próprio ID e posição. Isso funciona
/// automaticamente em qualquer cena/fase nova, sem configuração manual.
/// </summary>
public class CheckpointState : MonoBehaviour
{
    public static CheckpointState instance { get; private set; }

    public string CurrentCheckpointId { get; private set; } = string.Empty;

    // id do checkpoint -> posição no mundo. Repopulado a cada cena pelos
    // próprios CheckpointTrigger presentes nela (ver RegisterCheckpoint).
    private readonly Dictionary<string, Vector3> checkpointPositions =
        new Dictionary<string, Vector3>();

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

    /// <summary>
    /// Chamado por cada CheckpointTrigger no Awake/OnEnable para informar
    /// onde ele está. Sobrescreve a posição se o mesmo ID já existir
    /// (cobre o caso de recarregar a mesma cena).
    /// </summary>
    public void RegisterCheckpoint(string checkpointId, Vector3 position)
    {
        if (string.IsNullOrEmpty(checkpointId))
            return;

        checkpointPositions[checkpointId] = position;
    }

    /// <summary>
    /// Remove o registro de um checkpoint (chamado pelo CheckpointTrigger ao
    /// ser destruído), para não acumular posições de cenas antigas que não
    /// existem mais e poderiam, em tese, colidir com IDs reaproveitados.
    /// </summary>
    public void UnregisterCheckpoint(string checkpointId)
    {
        if (string.IsNullOrEmpty(checkpointId))
            return;

        checkpointPositions.Remove(checkpointId);
    }

    public void SetCheckpoint(string checkpointId)
    {
        if (string.IsNullOrEmpty(checkpointId))
            return;

        CurrentCheckpointId = checkpointId;
    }

    /// <summary>
    /// Retorna a posição do checkpoint ativo. Se o ID não estiver registrado
    /// (por exemplo, o checkpoint pertence a uma cena diferente da atual e
    /// ainda não foi carregada), retorna false em vez de mascarar o problema
    /// com Vector3.zero — quem chama decide o que fazer (ex.: manter a
    /// última posição conhecida, ou carregar a cena correta primeiro).
    /// </summary>
    public bool TryGetCheckpointPosition(out Vector3 position)
    {
        if (
            !string.IsNullOrEmpty(CurrentCheckpointId)
            && checkpointPositions.TryGetValue(CurrentCheckpointId, out position)
        )
        {
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Mantido por compatibilidade com o código existente (GameControler
    /// ainda chama este método). Use TryGetCheckpointPosition quando
    /// possível, pois ele deixa explícito o caso de "não encontrado".
    /// </summary>
    public Vector3 GetCheckpointPosition()
    {
        TryGetCheckpointPosition(out Vector3 position);
        return position;
    }

    public void Restaurar(string checkpointId)
    {
        CurrentCheckpointId = checkpointId ?? string.Empty;
    }

    /// <summary>
    /// True apenas se houver um checkpoint ativo E a posição dele já estiver
    /// registrada (ou seja, a cena correspondente está carregada). Antes,
    /// HasCheckpoint() só checava se CurrentCheckpointId não era vazio,
    /// o que permitia "ter checkpoint" sem ter posição válida.
    /// </summary>
    public bool HasCheckpoint()
    {
        return !string.IsNullOrEmpty(CurrentCheckpointId)
            && checkpointPositions.ContainsKey(CurrentCheckpointId);
    }
}
