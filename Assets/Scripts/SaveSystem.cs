using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Gerencia o salvamento e o carregamento do estado do jogador em um arquivo JSON local.
/// Deve ser anexado ao mesmo GameObject que possui Health e Inventory.
///
/// Teclas:
///   F5 — salva o jogo
///   F9 — carrega o jogo
/// </summary>
public class SaveSystem : MonoBehaviour
{
    // Caminho completo do arquivo de save, montado uma única vez no Awake
    private string caminhoArquivo;

    // Referências aos componentes do mesmo GameObject
    private Health health;
    private Inventory inventory;

    // -------------------------------------------------------------------------
    // INICIALIZAÇÃO
    // -------------------------------------------------------------------------

    private void Awake()
    {
        // Application.persistentDataPath aponta para a pasta de dados do app
        // no sistema operacional do jogador (Documents, AppData, etc.)
        caminhoArquivo = Path.Combine(Application.persistentDataPath, "savegame.json");

        health    = GetComponent<Health>();
        inventory = GetComponent<Inventory>();

        if (health == null)
            Debug.LogError("[SaveSystem] Health não encontrado neste GameObject.");

        if (inventory == null)
            Debug.LogError("[SaveSystem] Inventory não encontrado neste GameObject.");
    }

    // -------------------------------------------------------------------------
    // INPUT
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
            Salvar();

        if (Input.GetKeyDown(KeyCode.F9))
            Carregar();
    }

    // -------------------------------------------------------------------------
    // SALVAR
    // -------------------------------------------------------------------------

    /// <summary>
    /// Coleta o estado atual do jogador, converte para JSON e grava no disco.
    /// </summary>
    private void Salvar()
    {
        if (health == null || inventory == null) return;

        // Monta o objeto de dados com o estado atual
        SaveData dados = new SaveData();
        dados.vida = health.VidaAtual;
        dados.posX = transform.position.x;
        dados.posY = transform.position.y;
        dados.posZ = transform.position.z;

        // Copia a lista de itens para o SaveData (IReadOnlyList → nova List)
        dados.itens = new List<ItemType>(inventory.Itens);

        // Serializa para JSON e grava no arquivo
        string json = JsonUtility.ToJson(dados, prettyPrint: true);
        File.WriteAllText(caminhoArquivo, json);

        Debug.Log($"[SaveSystem] Jogo salvo em: {caminhoArquivo}");
    }

    // -------------------------------------------------------------------------
    // CARREGAR
    // -------------------------------------------------------------------------

    /// <summary>
    /// Lê o arquivo de save, reconstrói o SaveData e restaura o estado do jogador.
    /// </summary>
    private void Carregar()
    {
        if (health == null || inventory == null) return;

        if (!File.Exists(caminhoArquivo))
        {
            Debug.LogWarning("[SaveSystem] Nenhum arquivo de save encontrado. Jogue e salve com F5 primeiro.");
            return;
        }

        // Lê o JSON e reconstrói o SaveData
        string json = File.ReadAllText(caminhoArquivo);
        SaveData dados = JsonUtility.FromJson<SaveData>(json);

        // --- Restaura a posição ---
        // CharacterController sobrescreve mudanças diretas em transform.position,
        // então precisa ser desativado antes de mover e reativado depois.
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        transform.position = new Vector3(dados.posX, dados.posY, dados.posZ);

        if (controller != null)
            controller.enabled = true;

        // --- Restaura a vida ---
        // Calcula a diferença entre o valor salvo e o atual e aplica cura ou
        // dano conforme o sinal, restaurando o valor exato sem depender de
        // preencher até o máximo antes.
        float diferenca = dados.vida - health.VidaAtual;
        if (diferenca > 0f)
            health.Curar(diferenca);
        else if (diferenca < 0f)
            health.TomarDano(-diferenca);

        // --- Restaura o inventário ---
        // Remove todos os itens presentes antes de repovoar,
        // usando RemoveItem em loop até não restar nenhum de cada tipo.
        foreach (ItemType tipo in System.Enum.GetValues(typeof(ItemType)))
        {
            while (inventory.RemoveItem(tipo)) { }
        }

        // Adiciona os itens salvos um a um
        if (dados.itens != null)
        {
            foreach (ItemType item in dados.itens)
                inventory.AddItem(item);
        }

        Debug.Log($"[SaveSystem] Jogo carregado de: {caminhoArquivo}");
    }
}
