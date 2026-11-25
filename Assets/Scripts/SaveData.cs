using System.Collections.Generic;

/// <summary>
/// Estrutura de dados que representa o estado do jogador em um momento salvo.
/// Marcada como Serializable para que o JsonUtility consiga converter para JSON e vice-versa.
/// NÃO herda de MonoBehaviour — é um objeto de dados puro, sem comportamento Unity.
/// </summary>
[System.Serializable]
public class SaveData
{
    // Vida do jogador no momento do salvamento
    public float vida;

    // Posição do jogador decomposta em três floats separados.
    // Vector3 não é serializado corretamente pelo JsonUtility em todas as versões do Unity,
    // por isso usamos floats individuais.
    public float posX;
    public float posY;
    public float posZ;

    // Lista de itens que estavam no inventário no momento do salvamento.
    // Pocoes nunca aparecem aqui porque são consumidas imediatamente ao ser coletadas.
    public List<ItemType> itens;
}
