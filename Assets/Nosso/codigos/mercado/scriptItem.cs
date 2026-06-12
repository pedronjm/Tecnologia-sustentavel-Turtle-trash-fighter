using UnityEngine;

/// <summary>
/// ScriptableObject que define um item vendido no mercado.
/// Crie um via: botão direito no Project → Create → Mercado → Item
/// </summary>
[CreateAssetMenu(fileName = "NovoItem", menuName = "Mercado/Item")]
public class ItemMercado : ScriptableObject
{
    // ─────────────────────────────────────────────────────────────────────────
    // Identidade
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Identidade")]
    [Tooltip("ID único para persistência (nunca altere após criar saves).")]
    public string itemId;
    public string nomeExibido;
    [TextArea] public string descricao;
    public Sprite icone;

    // ─────────────────────────────────────────────────────────────────────────
    // Economia
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Economia")]
    public int custo;

    [Tooltip("False = item de uso único (some após compra). True = pode comprar sempre.")]
    public bool infinito = false;

    // ─────────────────────────────────────────────────────────────────────────
    // Tipo e efeito
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Tipo do Item")]
    public TipoItem tipo;

    [Header("Valores do Efeito")]
    [Tooltip("Vida recuperada (RecuperarVida) ou vida máxima adicionada (MaisVida).")]
    public float valorVida;

    [Tooltip("Dano adicional permanente.")]
    public float valorDano;

    [Tooltip("Velocidade adicional permanente.")]
    public float valorVelocidade;
}

public enum TipoItem
{
    RecuperarVida,          // cura o jogador agora
    MaisVidaMaxima,         // aumenta HP máximo permanentemente
    MaisDano,               // aumenta dano permanentemente
    MaisVelocidade,         // aumenta velocidade permanentemente
    ColetavelEspecial,      // entrega um coletável / chave ao jogador
}