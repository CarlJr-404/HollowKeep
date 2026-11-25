using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla o HUD do jogador.
/// Deve ser anexado ao Canvas da cena.
/// Arraste as referências no Inspector.
/// </summary>
public class HUDController : MonoBehaviour
{
    // Barra de vida do jogador (valor entre 0 e 1)
    [SerializeField] private Slider barraVida;

    // Texto que exibe a quantidade de itens no inventário
    [SerializeField] private TMP_Text textoItens;

    // Referência ao componente Health do jogador
    [SerializeField] private Health health;

    // Referência ao componente Inventory do jogador
    [SerializeField] private Inventory inventory;

    private void Update()
    {
        if (health != null)
            barraVida.value = health.VidaAtual / health.VidaMaxima;

        if (inventory != null)
            textoItens.text = "Itens: " + inventory.TotalItens;
    }
}
