using System.Collections;
using UnityEngine;

// Anexe este script ao objeto ZonaTrigger (filho da Porta).
// O ZonaTrigger deve ter um Box Collider com "Is Trigger" marcado.
public class PortaTrancada : MonoBehaviour
{
    [SerializeField] private Transform porta; // Objeto Porta (o pai) que afunda ao abrir

    private bool jaAberta = false; // Impede que a porta abra mais de uma vez

    private void OnTriggerEnter(Collider other)
    {
        // Ignora qualquer coisa que não seja o player
        if (!other.CompareTag("Player"))
            return;

        // Porta já foi aberta anteriormente — não faz nada
        if (jaAberta)
            return;

        Inventory inventario = other.GetComponent<Inventory>();

        if (inventario == null)
        {
            Debug.LogWarning("[Porta] O player não possui o componente Inventory.");
            return;
        }

        if (inventario.HasItem(ItemType.Chave))
        {
            jaAberta = true;
            Debug.Log("[Porta] Aberta com a chave!");
            StartCoroutine(AbrirPorta());
        }
        else
        {
            Debug.Log("[Porta] Trancada — você precisa da Chave.");
        }
    }

    // Move a porta para baixo (afundando no chão) ao longo de ~1 segundo
    private IEnumerator AbrirPorta()
    {
        float duracao = 1f;
        float tempo = 0f;

        Vector3 posicaoInicial = porta.position;
        // Desce uma distância equivalente à escala Y da porta para sumir no chão
        Vector3 posicaoFinal = posicaoInicial + Vector3.down * porta.localScale.y;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            porta.position = Vector3.Lerp(posicaoInicial, posicaoFinal, tempo / duracao);
            yield return null;
        }

        // Garante posição exata no final da animação
        porta.position = posicaoFinal;
    }
}
