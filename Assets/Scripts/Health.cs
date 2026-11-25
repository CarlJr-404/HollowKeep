using UnityEngine;

/// <summary>
/// Componente independente de vida. Pode ser anexado a qualquer GameObject
/// (jogador, inimigo, objeto destrutível) sem depender de outros scripts.
/// </summary>
public class Health : MonoBehaviour
{
    // Vida máxima configurável pelo Inspector
    [Tooltip("Vida máxima do objeto")]
    [SerializeField] private float vidaMaxima = 100f;

    // Vida atual — começa em 0 e é inicializada no Start
    [SerializeField] private float vidaAtual = 0f;

    // Impede que morte seja processada mais de uma vez
    private bool estaMorto = false;

    // -------------------------------------------------------------------------
    // INICIALIZAÇÃO
    // -------------------------------------------------------------------------

    private void Start()
    {
        // Se vidaAtual não foi definida externamente, inicia com vida cheia
        if (vidaAtual == 0f)
            vidaAtual = vidaMaxima;
    }

    // -------------------------------------------------------------------------
    // API PÚBLICA
    // -------------------------------------------------------------------------

    /// <summary>
    /// Restaura <paramref name="quantidade"/> de vida sem ultrapassar vidaMaxima.
    /// </summary>
    public void Curar(float quantidade)
    {
        if (estaMorto) return;

        vidaAtual = Mathf.Min(vidaAtual + quantidade, vidaMaxima);
        Debug.Log($"[Health] {name} curado em {quantidade} — vida atual: {vidaAtual}/{vidaMaxima}");
    }

    /// <summary>
    /// Aplica <paramref name="quantidade"/> de dano sem deixar vidaAtual abaixo de 0.
    /// Processa a morte caso a vida chegue a zero.
    /// </summary>
    public void TomarDano(float quantidade)
    {
        if (estaMorto) return;

        vidaAtual = Mathf.Max(vidaAtual - quantidade, 0f);
        Debug.Log($"[Health] {name} sofreu {quantidade} de dano — vida atual: {vidaAtual}/{vidaMaxima}");

        if (vidaAtual <= 0f)
            Morrer();
    }

    /// <summary>Retorna a vida atual (0 a vidaMaxima).</summary>
    public float VidaAtual => vidaAtual;

    /// <summary>Retorna a vida máxima configurada.</summary>
    public float VidaMaxima => vidaMaxima;

    /// <summary>Retorna true se o objeto estiver morto.</summary>
    public bool EstaMorto => estaMorto;

    // -------------------------------------------------------------------------
    // MORTE
    // -------------------------------------------------------------------------

    private void Morrer()
    {
        estaMorto = true;
        Debug.Log($"[Health] {name} morreu.");

        // Ponto de extensão: adicione animação de morte, Game Over, destruição, etc.
    }
}
