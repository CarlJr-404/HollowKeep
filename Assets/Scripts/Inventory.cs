using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerencia o inventário do jogador.
/// Deve ser anexado ao mesmo GameObject que possui o componente Health.
/// </summary>
public class Inventory : MonoBehaviour
{
    // Quantidade de vida restaurada ao usar uma poção
    private const float CuraPoção = 30f;

    // Lista interna dos itens atualmente no inventário
    private List<ItemType> itens = new List<ItemType>();

    public int TotalItens => itens.Count;

    /// <summary>Retorna uma cópia somente-leitura dos itens para serialização externa.</summary>
    public IReadOnlyList<ItemType> Itens => itens.AsReadOnly();

    /// <summary>
    /// Adiciona um item ao inventário.
    /// Poções são usadas imediatamente: curam o jogador e são removidas.
    /// </summary>
    public void AddItem(ItemType item)
    {
        itens.Add(item);
        Debug.Log($"[Inventory] Item coletado: {item} | Total no inventário: {itens.Count}");

        // Poções têm efeito imediato — cura e auto-remove
        if (item == ItemType.Pocao)
            UsarPocao();
    }

    /// <summary>
    /// Retorna true se o inventário contiver pelo menos uma unidade do item informado.
    /// </summary>
    public bool HasItem(ItemType item)
    {
        return itens.Contains(item);
    }

    /// <summary>
    /// Remove uma unidade do item informado do inventário.
    /// Retorna true se a remoção foi bem-sucedida, false se o item não estava presente.
    /// </summary>
    public bool RemoveItem(ItemType item)
    {
        bool removido = itens.Remove(item);

        if (removido)
            Debug.Log($"[Inventory] Item removido: {item} | Total no inventário: {itens.Count}");
        else
            Debug.LogWarning($"[Inventory] Tentativa de remover '{item}', mas ele não estava no inventário.");

        return removido;
    }

    // Aplica a cura via Health e remove a poção do inventário
    private void UsarPocao()
    {
        Health health = GetComponent<Health>();
        if (health != null)
            health.Curar(CuraPoção);
        else
            Debug.LogWarning("[Inventory] Health não encontrado neste GameObject. Poção não curou.");

        // Remove a poção que acabou de ser adicionada
        itens.Remove(ItemType.Pocao);
        Debug.Log($"[Inventory] Poção usada — {CuraPoção} HP restaurados. Poção removida do inventário.");
    }
}
