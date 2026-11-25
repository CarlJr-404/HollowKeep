# Hollow Keep

**Um jogo de dungeon em 3D desenvolvido em Unity (C#), com foco em sistemas de gameplay construídos do zero.**

🎮 **Jogue no navegador:** [carljr-404.itch.io/hollow-keep](https://carljr-404.itch.io/hollow-keep)

---

## Sobre o projeto

Hollow Keep é um protótipo jogável onde o jogador explora uma masmorra, coleta itens, escapa de um inimigo que o persegue e usa uma chave para abrir a porta que dá acesso adiante. O projeto foi construído para demonstrar, na prática, a implementação de sistemas de jogo interligados — desde a inteligência artificial do inimigo até um sistema completo de salvar e carregar o progresso.

O objetivo não foi criar um jogo grande, e sim um conjunto de sistemas bem-feitos, organizados e que se comunicam de forma limpa. Cada mecânica foi implementada e testada individualmente antes de ser integrada às demais.

---

## O que o jogo tem

- **Exploração em terceira pessoa** por uma masmorra com iluminação ambientada por tochas.
- **Inimigo com IA** que patrulha e persegue o jogador ao detectá-lo.
- **Coleta de itens e inventário** — poção que cura e chave necessária para progredir.
- **HUD reativa** com barra de vida e contador de itens em tempo real.
- **Porta trancada** que só abre com a chave no inventário.
- **Salvar e carregar** o progresso (posição, vida e itens).

---

## Sistemas implementados (visão técnica)

| Sistema | Descrição | Conceitos aplicados |
|---|---|---|
| **IA do inimigo** | Máquina de estados (Patrulha → Alerta → Perseguição) com detecção por campo de visão | Máquina de estados finitos, NavMesh/pathfinding |
| **Vida** | Componente independente de vida, cura e dano | Encapsulamento, separação de responsabilidades |
| **Inventário** | Coleta por trigger, itens tipados por enum | Type safety, propriedades somente-leitura |
| **HUD** | Interface que exibe vida e itens em tempo real | Fluxo de dados em mão única (UI reativa) |
| **Porta trancada** | Abre apenas com a chave, com animação por código | Corrotina, interpolação (Lerp) |
| **Save/Load** | Salva e restaura o estado do jogo em arquivo | Serialização para JSON |

Os sistemas se comunicam por interfaces limpas (por exemplo, a porta consulta o inventário sem manipulá-lo diretamente), e o código foi mantido enxuto, sem duplicações ou trechos não utilizados.

---

## Controles

| Ação | Tecla |
|---|---|
| Mover | W A S D |
| Correr | Shift |
| Pular | Espaço |
| Câmera | Mouse |
| Salvar | F5 |
| Carregar | F9 |

---

## Como rodar o projeto

**Para jogar:** use a versão no navegador pelo [itch.io](https://carljr-404.itch.io/hollow-keep) — não precisa instalar nada.

**Para abrir o projeto na Unity:**
1. Clone o repositório: `git clone https://github.com/CarlJr-404/HollowKeep.git`
2. Abra o projeto pela Unity Hub (Unity 2022.3.61f1 ou compatível).
3. Abra a cena `Assets/Scenes/HollowKeep`.
4. Pressione Play.

---

## Tecnologias

- **Engine:** Unity 2022.3 (URP)
- **Linguagem:** C#
- **Recursos:** NavMesh (IA/pathfinding), UI da Unity + TextMeshPro (HUD), corrotinas, serialização via JsonUtility
- **Movimentação:** Third Person Controller (Starter Assets da Unity)
- **Ferramentas:** desenvolvido com apoio de IA (Claude Code) na geração de código, com revisão e integração de cada sistema

---

## Autor

Desenvolvido por **Carlos Junior**

- itch.io: [carljr-404.itch.io](https://carljr-404.itch.io)
- GitHub: [CarlJr-404](https://github.com/CarlJr-404)
