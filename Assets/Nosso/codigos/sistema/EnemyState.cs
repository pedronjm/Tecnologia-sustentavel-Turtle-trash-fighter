using System.Collections.Generic;
using UnityEngine;

public class EnemyState : MonoBehaviour
{
    public static EnemyState instance { get; private set; }

    private readonly HashSet<string> allEnemyIds = new HashSet<string>();
    private readonly HashSet<string> deadEnemyIds = new HashSet<string>();

    public int TotalEnemies => allEnemyIds.Count;
    public int DeadEnemies => deadEnemyIds.Count;

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

    public void RegistrarInimigo(string id)
    {
        if (string.IsNullOrEmpty(id))
            return;

        allEnemyIds.Add(id);
    }

    public void MarcarMorto(string id)
    {
        if (string.IsNullOrEmpty(id))
            return;

        allEnemyIds.Add(id);
        deadEnemyIds.Add(id);
    }

    public bool EstaMorto(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return deadEnemyIds.Contains(id);
    }

    public List<string> GetDeadEnemyIds()
    {
        return new List<string>(deadEnemyIds);
    }

    public void CarregarInimigosMortos(IEnumerable<string> ids)
    {
        deadEnemyIds.Clear();

        if (ids != null)
        {
            foreach (var id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                    deadEnemyIds.Add(id);
            }
        }

        AplicarEstadoNaCena();
    }

    public void AplicarEstadoNaCena()
    {
        var inimigos = FindObjectsByType<enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var inimigo in inimigos)
        {
            var id = inimigo.GetSaveId();
            if (EstaMorto(id))
                Destroy(inimigo.gameObject);
        }
    }
}
