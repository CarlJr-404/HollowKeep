using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// IA de inimigo com máquina de estados: Patrulha → Alerta → Perseguição.
/// Requer: NavMeshAgent no mesmo GameObject.
/// Setup: atribua os waypoints e o Transform do jogador no Inspector.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    // =========================================================================
    // ENUMERAÇÃO DE ESTADOS
    // =========================================================================

    public enum EstadoInimigo
    {
        Patrulha,    // Caminha entre waypoints normalmente
        Alerta,      // Parou — ouviu algo suspeito, procura com o olhar
        Perseguicao  // Viu o jogador — corre para eliminar
    }

    // =========================================================================
    // CONFIGURAÇÕES — PATRULHA
    // =========================================================================

    [Header("Patrulha")]
    [Tooltip("Pontos de patrulha que o inimigo percorre em ordem (loop)")]
    [SerializeField] private Transform[] waypoints;

    [Tooltip("Velocidade de caminhada durante a patrulha")]
    [SerializeField] private float velocidadePatrulha = 2.5f;

    [Tooltip("Distância mínima para considerar que chegou ao waypoint")]
    [SerializeField] private float toleranciaWaypoint = 0.4f;

    [Tooltip("Tempo de espera (segundos) ao chegar em cada waypoint")]
    [SerializeField] private float tempoEsperaWaypoint = 1.5f;

    // =========================================================================
    // CONFIGURAÇÕES — CAMPO DE VISÃO
    // =========================================================================

    [Header("Campo de Visão")]
    [Tooltip("Distância máxima em que o inimigo enxerga o jogador")]
    [SerializeField] private float distanciaVisao = 12f;

    [Tooltip("Ângulo total do cone de visão (ex: 90 = 45° para cada lado)")]
    [SerializeField] private float anguloVisao = 90f;

    [Tooltip("Layer(s) que bloqueiam a linha de visão (paredes, obstáculos)")]
    [SerializeField] private LayerMask layerObstaculo;

    [Tooltip("Layer do jogador — usado no raycast de visão")]
    [SerializeField] private LayerMask layerJogador;

    // =========================================================================
    // CONFIGURAÇÕES — ALERTA
    // =========================================================================

    [Header("Alerta")]
    [Tooltip("Tempo (segundos) que o inimigo fica em alerta antes de retornar à patrulha")]
    [SerializeField] private float duracaoAlerta = 4f;

    [Tooltip("Velocidade de rotação ao procurar o jogador (graus/segundo)")]
    [SerializeField] private float velocidadeRotacaoAlerta = 60f;

    [Tooltip("Ângulos de varredura durante o alerta (inimigo olha para os lados)")]
    [SerializeField] private float anguloVarredura = 45f;

    // =========================================================================
    // CONFIGURAÇÕES — PERSEGUIÇÃO
    // =========================================================================

    [Header("Perseguição")]
    [Tooltip("Velocidade de corrida ao perseguir o jogador")]
    [SerializeField] private float velocidadePerseguicao = 5.5f;

    [Tooltip("Segundos sem contato visual até desistir da perseguição")]
    [SerializeField] private float tempoLimiteSemlinha = 3f;

    [Tooltip("Distância mínima do jogador para o inimigo parar de se aproximar")]
    [SerializeField] private float distanciaAtaque = 1.8f;

    // =========================================================================
    // REFERÊNCIAS EXTERNAS
    // =========================================================================

    [Header("Referências")]
    [Tooltip("Transform do personagem do jogador")]
    [SerializeField] private Transform jogador;

    [Tooltip("Ponto de origem do raycast de visão (olhos do inimigo)")]
    [SerializeField] private Transform pontoVisao;

    // =========================================================================
    // VARIÁVEIS PRIVADAS — COMPONENTES
    // =========================================================================

    private NavMeshAgent agente;
    private Animator animador; // opcional — atribuído automaticamente se existir

    // =========================================================================
    // VARIÁVEIS PRIVADAS — MÁQUINA DE ESTADOS
    // =========================================================================

    private EstadoInimigo estadoAtual;
    private EstadoInimigo EstadoAtual
    {
        get => estadoAtual;
        set
        {
            if (estadoAtual == value) return;
            SairEstado(estadoAtual);
            estadoAtual = value;
            EntrarEstado(estadoAtual);
        }
    }

    // =========================================================================
    // VARIÁVEIS PRIVADAS — PATRULHA
    // =========================================================================

    private int indiceWaypoint = 0;       // Waypoint atual na sequência
    private bool esperandoWaypoint = false; // Está aguardando no ponto?

    // =========================================================================
    // VARIÁVEIS PRIVADAS — ALERTA
    // =========================================================================

    private float timerAlerta = 0f;
    private Vector3 posicaoSuspeita;         // Última posição suspeita ouvida/vista
    private Coroutine rotinaBuscaAtiva;      // Corrotina de varredura visual

    // =========================================================================
    // VARIÁVEIS PRIVADAS — PERSEGUIÇÃO
    // =========================================================================

    private float timerSemContato = 0f;      // Contador de tempo sem ver o jogador
    private Vector3 ultimaPosicaoConhecida;  // Última posição onde o jogador foi visto

    // =========================================================================
    // UNITY — INICIALIZAÇÃO
    // =========================================================================

    private void Awake()
    {
        agente   = GetComponent<NavMeshAgent>();
        animador = GetComponent<Animator>(); // null se não houver Animator

        // Usa o próprio transform como ponto de visão se não for atribuído
        if (pontoVisao == null)
            pontoVisao = transform;

        // Busca o jogador pela tag se não foi atribuído no Inspector
        if (jogador == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) jogador = obj.transform;
        }

        ValidarWaypoints();
    }

    private void Start()
    {
        // Inicia diretamente na patrulha
        estadoAtual = EstadoInimigo.Patrulha; // atribuição direta para não disparar SairEstado
        EntrarEstado(EstadoInimigo.Patrulha);
    }

    // =========================================================================
    // UNITY — LOOP PRINCIPAL
    // =========================================================================

    private void Update()
    {
        switch (EstadoAtual)
        {
            case EstadoInimigo.Patrulha:    AtualizarPatrulha();    break;
            case EstadoInimigo.Alerta:      AtualizarAlerta();      break;
            case EstadoInimigo.Perseguicao: AtualizarPerseguicao(); break;
        }
    }

    // =========================================================================
    // MÁQUINA DE ESTADOS — ENTRAR / SAIR
    // =========================================================================

    /// <summary>Configurações aplicadas ao entrar em cada estado.</summary>
    private void EntrarEstado(EstadoInimigo estado)
    {
        Debug.Log($"[EnemyAI] {name} → {estado}");

        switch (estado)
        {
            case EstadoInimigo.Patrulha:
                agente.speed    = velocidadePatrulha;
                agente.isStopped = false;
                IrParaWaypointAtual();
                AtualizarAnimador("caminhar", true);
                AtualizarAnimador("correr", false);
                break;

            case EstadoInimigo.Alerta:
                agente.isStopped = true;        // Para no lugar
                timerAlerta = duracaoAlerta;
                AtualizarAnimador("caminhar", false);
                AtualizarAnimador("alerta", true);
                // Inicia a rotina de varredura (olhar para os lados)
                if (rotinaBuscaAtiva != null) StopCoroutine(rotinaBuscaAtiva);
                rotinaBuscaAtiva = StartCoroutine(VarreuraVisual());
                break;

            case EstadoInimigo.Perseguicao:
                agente.speed    = velocidadePerseguicao;
                agente.isStopped = false;
                timerSemContato = 0f;
                AtualizarAnimador("caminhar", false);
                AtualizarAnimador("correr", true);
                AtualizarAnimador("alerta", false);
                break;
        }
    }

    /// <summary>Limpeza executada ao sair de cada estado.</summary>
    private void SairEstado(EstadoInimigo estado)
    {
        switch (estado)
        {
            case EstadoInimigo.Alerta:
                // Interrompe a varredura se ainda estiver rodando
                if (rotinaBuscaAtiva != null)
                {
                    StopCoroutine(rotinaBuscaAtiva);
                    rotinaBuscaAtiva = null;
                }
                AtualizarAnimador("alerta", false);
                break;
        }
    }

    // =========================================================================
    // ESTADO — PATRULHA
    // =========================================================================

    private void AtualizarPatrulha()
    {
        // Verifica campo de visão a cada frame — prioridade máxima
        if (JogadorNoCampoDeVisao())
        {
            ultimaPosicaoConhecida = jogador.position;
            EstadoAtual = EstadoInimigo.Perseguicao;
            return;
        }

        if (esperandoWaypoint) return; // aguardando o timer no waypoint

        // Chegou perto o suficiente do waypoint atual?
        if (!agente.pathPending && agente.remainingDistance <= toleranciaWaypoint)
            StartCoroutine(EsperarEAvancar());
    }

    /// <summary>Aguarda no waypoint e avança para o próximo.</summary>
    private IEnumerator EsperarEAvancar()
    {
        esperandoWaypoint = true;
        agente.isStopped  = true;

        yield return new WaitForSeconds(tempoEsperaWaypoint);

        esperandoWaypoint = false;
        agente.isStopped  = false;

        // Avança para o próximo waypoint em loop circular
        indiceWaypoint = (indiceWaypoint + 1) % waypoints.Length;
        IrParaWaypointAtual();
    }

    private void IrParaWaypointAtual()
    {
        if (waypoints.Length == 0) return;
        agente.SetDestination(waypoints[indiceWaypoint].position);
    }

    // =========================================================================
    // ESTADO — ALERTA
    // =========================================================================

    private void AtualizarAlerta()
    {
        // Se enxergar o jogador enquanto está em alerta, persegue imediatamente
        if (JogadorNoCampoDeVisao())
        {
            ultimaPosicaoConhecida = jogador.position;
            EstadoAtual = EstadoInimigo.Perseguicao;
            return;
        }

        // Decrementa o timer de alerta
        timerAlerta -= Time.deltaTime;
        if (timerAlerta <= 0f)
        {
            // Tempo de alerta esgotado — volta a patrulhar
            EstadoAtual = EstadoInimigo.Patrulha;
        }
    }

    /// <summary>
    /// Corrotina de varredura: o inimigo olha para os lados enquanto está em alerta.
    /// Gira para um lado, aguarda, gira para o outro, repete até sair do estado.
    /// </summary>
    private IEnumerator VarreuraVisual()
    {
        Quaternion rotacaoOriginal = transform.rotation;

        while (true)
        {
            // Gira para a direita
            yield return RotacionarAte(rotacaoOriginal * Quaternion.Euler(0f,  anguloVarredura, 0f));
            yield return new WaitForSeconds(0.4f);

            // Gira para a esquerda
            yield return RotacionarAte(rotacaoOriginal * Quaternion.Euler(0f, -anguloVarredura, 0f));
            yield return new WaitForSeconds(0.4f);

            // Volta ao centro
            yield return RotacionarAte(rotacaoOriginal);
            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>Rotaciona suavemente até a orientação alvo.</summary>
    private IEnumerator RotacionarAte(Quaternion alvo)
    {
        while (Quaternion.Angle(transform.rotation, alvo) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, alvo, velocidadeRotacaoAlerta * Time.deltaTime);
            yield return null;
        }
        transform.rotation = alvo;
    }

    // =========================================================================
    // ESTADO — PERSEGUIÇÃO
    // =========================================================================

    private void AtualizarPerseguicao()
    {
        bool veJogador = JogadorNoCampoDeVisao();

        if (veJogador)
        {
            // Reseta o contador e atualiza a última posição conhecida
            timerSemContato        = 0f;
            ultimaPosicaoConhecida = jogador.position;

            float distancia = Vector3.Distance(transform.position, jogador.position);

            // Mantém distância mínima de ataque — para e enfrenta o jogador
            if (distancia <= distanciaAtaque)
            {
                agente.isStopped = true;
                EnfrentarAlvo(jogador.position);
                // Aqui você pode disparar a animação/lógica de ataque:
                // AtualizarAnimador("atacar", true);
            }
            else
            {
                agente.isStopped = false;
                agente.SetDestination(jogador.position);
            }
        }
        else
        {
            // Perdeu a linha de visão — vai até a última posição conhecida
            agente.isStopped = false;
            agente.SetDestination(ultimaPosicaoConhecida);

            timerSemContato += Time.deltaTime;

            if (timerSemContato >= tempoLimiteSemlinha)
            {
                // Perdeu o jogador definitivamente — entra em alerta
                posicaoSuspeita = ultimaPosicaoConhecida;
                EstadoAtual = EstadoInimigo.Alerta;
            }
        }
    }

    /// <summary>Rotaciona o inimigo para encarar uma posição alvo instantaneamente.</summary>
    private void EnfrentarAlvo(Vector3 alvo)
    {
        Vector3 direcao = (alvo - transform.position).normalized;
        direcao.y = 0f; // mantém o inimigo nivelado (sem inclinar para cima/baixo)

        if (direcao == Vector3.zero) return;

        Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, rotacaoAlvo, velocidadeRotacaoAlerta * Time.deltaTime);
    }

    // =========================================================================
    // CAMPO DE VISÃO
    // =========================================================================

    /// <summary>
    /// Retorna true se o jogador estiver dentro do cone de visão E sem obstáculo
    /// entre o inimigo e ele.
    /// </summary>
    private bool JogadorNoCampoDeVisao()
    {
        if (jogador == null) return false;

        Vector3 direcaoParaJogador = (jogador.position - pontoVisao.position);
        float distancia = direcaoParaJogador.magnitude;

        if (distancia > distanciaVisao) return false;

        float angulo = Vector3.Angle(pontoVisao.forward, direcaoParaJogador);
        if (angulo > anguloVisao * 0.5f) return false;

        return true;
    }

    // =========================================================================
    // API PÚBLICA — ALERTAR EXTERNAMENTE
    // =========================================================================

    /// <summary>
    /// Permite que outros sistemas (colisão de som, armadilhas) coloquem o
    /// inimigo em alerta a partir de uma posição suspeita.
    /// </summary>
    public void OuvirRuido(Vector3 posicao)
    {
        if (EstadoAtual == EstadoInimigo.Perseguicao) return; // já em perseguição, ignora

        posicaoSuspeita = posicao;

        // Vai investigar o ponto do ruído antes de entrar em alerta estático
        agente.isStopped = false;
        agente.SetDestination(posicaoSuspeita);
        EstadoAtual = EstadoInimigo.Alerta;
    }

    // =========================================================================
    // UTILITÁRIOS
    // =========================================================================

    /// <summary>Dispara um parâmetro bool no Animator se ele existir.</summary>
    private void AtualizarAnimador(string parametro, bool valor)
    {
        if (animador == null) return;
        animador.SetBool(parametro, valor);
    }

    private void ValidarWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0)
            Debug.LogWarning($"[EnemyAI] {name}: nenhum waypoint atribuído. O inimigo ficará parado.");
    }

    // =========================================================================
    // GIZMOS — VISUALIZAÇÃO DO CONE DE VISÃO NO EDITOR
    // =========================================================================

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (pontoVisao == null) pontoVisao = transform;

        // Esfera de alcance máximo
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawSphere(pontoVisao.position, distanciaVisao);

        // Bordas do cone de visão
        Gizmos.color = Color.yellow;
        float metadeAngulo = anguloVisao * 0.5f;
        Vector3 bordaDireita  = Quaternion.Euler(0,  metadeAngulo, 0) * pontoVisao.forward * distanciaVisao;
        Vector3 bordaEsquerda = Quaternion.Euler(0, -metadeAngulo, 0) * pontoVisao.forward * distanciaVisao;
        Gizmos.DrawLine(pontoVisao.position, pontoVisao.position + bordaDireita);
        Gizmos.DrawLine(pontoVisao.position, pontoVisao.position + bordaEsquerda);
        Gizmos.DrawLine(pontoVisao.position + bordaEsquerda, pontoVisao.position + bordaDireita);

        // Linha reta de visão frontal
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pontoVisao.position, pontoVisao.position + pontoVisao.forward * distanciaVisao);

        // Distância de ataque
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);

        // Waypoints
        if (waypoints == null) return;
        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);
            if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
        // Liga o último waypoint ao primeiro
        if (waypoints.Length > 1 && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
    }
#endif
}
