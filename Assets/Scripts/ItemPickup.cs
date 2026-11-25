using UnityEngine;

/// <summary>
/// Representa um item físico no mundo que pode ser coletado pelo jogador.
/// Requer um Collider com "Is Trigger" ativado no GameObject.
/// </summary>
public class ItemPickup : MonoBehaviour
{
    [Tooltip("Tipo do item que este objeto representa. Escolha no Inspector.")]
    public ItemType tipoItem;

    /// <summary>
    /// Detecta quando o jogador entra no trigger.
    /// Adiciona o item ao inventário e destrói este objeto de cena.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se quem entrou no trigger é o jogador pela tag
        if (!other.CompareTag("Player"))
            return;

        // Busca o Inventory no GameObject do jogador
        Inventory inventario = other.GetComponent<Inventory>();

        if (inventario == null)
        {
            Debug.LogWarning($"[ItemPickup] O jogador não possui um componente Inventory! Item '{tipoItem}' não coletado.");
            return;
        }

        // Registra o item no inventário (poções já se auto-consomem dentro do Inventory)
        inventario.AddItem(tipoItem);

        // Remove o item do mundo após a coleta
        Destroy(gameObject);
    }
}
