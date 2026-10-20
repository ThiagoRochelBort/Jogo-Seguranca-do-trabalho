using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estado e regras do jogo. Não sabe nada de interface: a GameUI chama estes métodos.
/// Os valores públicos podem ser ajustados no Inspector para balancear.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Regras")]
    public int maxHealth = 5;
    [Tooltip("Quantos pares vão para a mesa (metade NR ↔ tema, metade risco ↔ EPI). Cada par vale +1 na Auditoria")]
    public int pairsPerGame = 10;
    [Tooltip("Quantos turnos do próprio jogador a habilidade fica recarregando após o uso")]
    public int abilityRecharge = 1;
    [Tooltip("A cada quantas rodadas acontece um evento na empresa (0 = nunca, o padrão). " +
        "O campo tem nome novo de propósito: cenas antigas guardavam o 3 no nome 'eventEvery', que agora é ignorado")]
    public int eventEveryRounds = 0;
    [Tooltip("Nome do JSON dentro de Assets/Resources (sem .json)")]
    public string cardsResource = "cards";

    [Header("Memorização")]
    [Tooltip("Segundos com todas as cartas viradas para cima antes da partida começar (0 = sem prévia)")]
    [SerializeField] float previewSeconds = 8;
    public float PreviewSeconds { get { return previewSeconds; } }

    [Header("Desafios")]
    [Tooltip("Peso de cada tipo no sorteio do desafio (0 = nunca). Nunca sai o mesmo tipo duas vezes seguidas")]
    [SerializeField] ChallengeWeights challengeWeights = new ChallengeWeights();
    [Tooltip("Erros permitidos num desafio prático quando a carta não define maxMistakes")]
    [SerializeField] int defaultMaxMistakes = 2;

    public int Health { get; private set; }
    public int Audit { get; private set; }          // pares encontrados
    public int AuditGoal { get; private set; }      // total de pares na mesa
    public int Round { get; private set; }
    public int MemoryMisses { get; private set; }
    public int ChallengesWon { get; private set; }
    public int ChallengesLost { get; private set; }
    public bool ShieldActive { get; set; }
    public bool EventPending { get; set; }

    public List<Player> Players { get; private set; }
    public int CurrentIndex { get; private set; }
    public Player Current { get { return Players[CurrentIndex]; } }
    public List<MemoryTile> Board { get; private set; }
    public CardDeck Events { get; private set; }
    readonly Dictionary<CardType, CardDeck> challengeDecks = new Dictionary<CardType, CardDeck>();
    CardType lastChallengeType;
    bool hasLastChallenge;
    public CardData CurrentCard { get; private set; }

    public bool IsOver { get { return Health <= 0 || Audit >= AuditGoal; } }
    public bool Won { get { return Health > 0 && Audit >= AuditGoal; } }

    static readonly Role[] RoleOrder =
    {
        Role.TecnicoSeguranca, Role.MembroCIPA, Role.Brigadista, Role.Engenheiro
    };

    void Awake()
    {
        Players = new List<Player>();
        Board = new List<MemoryTile>();
    }

    /// <summary>Prepara uma nova partida. Retorna false se faltarem pares ou desafios.</summary>
    public bool Setup(int playerCount)
    {
        Random.InitState(System.Environment.TickCount); // cada partida embaralha de um jeito diferente
        Players.Clear();
        for (int i = 0; i < playerCount; i++)
            Players.Add(new Player("Jogador " + (i + 1), RoleOrder[i % RoleOrder.Length]));

        Health = maxHealth;
        Audit = 0;
        Round = 1;
        MemoryMisses = 0;
        ChallengesWon = 0;
        ChallengesLost = 0;
        CurrentIndex = 0;
        ShieldActive = false;
        EventPending = false;

        hasLastChallenge = false;

        var data = CardDeck.LoadFromResources(cardsResource);
        var byType = new Dictionary<CardType, List<CardData>>();
        var events = new List<CardData>();
        foreach (var c in data.cards)
        {
            if (c.Type == CardType.Event)
            {
                events.Add(c);
                continue;
            }
            if (!IsPlayable(c.Type)) continue;
            if (!c.HasRequiredData())
            {
                Debug.LogWarning("Carta " + c.id + " (" + c.type + ") ignorada: faltam campos ou há índice fora da lista.");
                continue;
            }
            if (!byType.ContainsKey(c.Type)) byType[c.Type] = new List<CardData>();
            byType[c.Type].Add(c);
        }

        challengeDecks.Clear();
        foreach (var entry in byType) challengeDecks[entry.Key] = new CardDeck(entry.Value);
        Events = new CardDeck(events);

        BuildBoard(data.pairs);
        return Board.Count >= 4 && challengeDecks.Count > 0;
    }

    /// <summary>Tipos que entram no sorteio de desafio (evento tem baralho próprio).</summary>
    static bool IsPlayable(CardType t)
    {
        return t != CardType.Event;
    }

    /// <summary>Erros permitidos num desafio prático.</summary>
    public int MaxMistakes(CardData c)
    {
        return c.maxMistakes > 0 ? c.maxMistakes : Mathf.Max(1, defaultMaxMistakes);
    }

    /// <summary>Sorteia os pares alternando NR e EPI e embaralha as cartas na mesa.</summary>
    void BuildBoard(List<PairData> pool)
    {
        var nr = new List<PairData>();
        var epi = new List<PairData>();
        foreach (var p in pool) (p.IsNr ? nr : epi).Add(p);
        CardDeck.Shuffle(nr);
        CardDeck.Shuffle(epi);

        var chosen = new List<PairData>();
        while (chosen.Count < pairsPerGame && (nr.Count > 0 || epi.Count > 0))
        {
            var from = (chosen.Count % 2 == 0 && nr.Count > 0) || epi.Count == 0 ? nr : epi;
            chosen.Add(from[from.Count - 1]);
            from.RemoveAt(from.Count - 1);
        }

        Board.Clear();
        foreach (var p in chosen)
        {
            Board.Add(new MemoryTile { pair = p, sideA = true });
            Board.Add(new MemoryTile { pair = p, sideA = false });
        }
        CardDeck.Shuffle(Board);
        AuditGoal = chosen.Count;
    }

    /// <summary>Confere duas cartas viradas. Se forem par, ficam abertas e a Auditoria sobe.</summary>
    public bool TryMatch(int first, int second)
    {
        var a = Board[first];
        var b = Board[second];
        if (a.pair == b.pair)
        {
            a.matched = b.matched = true;
            Audit++;
            return true;
        }
        MemoryMisses++;
        return false;
    }

    public CardData DrawChallenge()
    {
        ShieldActive = false;
        CurrentCard = null;
        var types = ChallengeTypes(false);
        if (types.Count == 0) types = ChallengeTypes(true); // só existe um tipo: pode repetir
        if (types.Count > 0)
        {
            var type = PickWeighted(types);
            CurrentCard = challengeDecks[type].Draw();
            lastChallengeType = type;
            hasLastChallenge = true;
        }
        return CurrentCard;
    }

    List<CardType> ChallengeTypes(bool allowRepeat)
    {
        var list = new List<CardType>();
        foreach (var type in challengeDecks.Keys)
        {
            if (challengeWeights.For(type) <= 0) continue;
            if (!allowRepeat && hasLastChallenge && type == lastChallengeType) continue;
            list.Add(type);
        }
        // Se todos os pesos forem 0, ainda assim sorteia entre os tipos que têm cartas
        if (list.Count == 0 && allowRepeat) list.AddRange(challengeDecks.Keys);
        return list;
    }

    CardType PickWeighted(List<CardType> types)
    {
        int total = 0;
        foreach (var t in types) total += Mathf.Max(0, challengeWeights.For(t));
        if (total <= 0) return types[Random.Range(0, types.Count)];

        int roll = Random.Range(0, total);
        foreach (var t in types)
        {
            roll -= Mathf.Max(0, challengeWeights.For(t));
            if (roll < 0) return t;
        }
        return types[types.Count - 1];
    }

    /// <summary>Resolve o desafio após um erro de memória. Retorna quanto Índice foi perdido.</summary>
    public int ResolveChallenge(CardData card, bool correct)
    {
        if (correct)
        {
            ChallengesWon++;
            return 0;
        }

        ChallengesLost++;
        if (ShieldActive)
        {
            ShieldActive = false;
            return 0;
        }

        int dmg = card.damage > 0 ? card.damage : 1;
        Health = Mathf.Max(0, Health - dmg);
        return dmg;
    }

    /// <summary>Aplica um evento. auditDelta positivo abre pares ainda escondidos; retorna os pares abertos.</summary>
    public List<PairData> ApplyEvent(CardData card)
    {
        Health = Mathf.Clamp(Health + card.healthDelta, 0, maxHealth);

        var revealed = new List<PairData>();
        for (int k = 0; k < card.auditDelta; k++)
        {
            var hidden = new List<MemoryTile>();
            foreach (var t in Board) if (!t.matched) hidden.Add(t);
            if (hidden.Count == 0) break;
            var pair = hidden[Random.Range(0, hidden.Count)].pair;
            foreach (var t in Board) if (t.pair == pair) t.matched = true;
            Audit++;
            revealed.Add(pair);
        }
        return revealed;
    }

    public void Heal(int amount)
    {
        Health = Mathf.Min(maxHealth, Health + amount);
    }

    public void UseAbility()
    {
        Current.cooldown = abilityRecharge + 1; // +1 porque o turno atual também desconta
    }

    public void EndTurn()
    {
        Current.Tick();
        CurrentIndex = (CurrentIndex + 1) % Players.Count;
        if (CurrentIndex == 0)
        {
            Round++;
            if (eventEveryRounds > 0 && (Round - 1) % eventEveryRounds == 0) EventPending = true;
        }
        ShieldActive = false;
    }
}

/// <summary>Peso de cada tipo de desafio no sorteio (valores do GDD: pergunta 35, os outros 10).</summary>
[System.Serializable]
public class ChallengeWeights
{
    public int question = 35;
    public int sequence = 10;
    public int epi = 10;
    public int wire = 10;
    public int hunt = 10;
    public int match = 10;
    public int lockout = 10;

    public int For(CardType t)
    {
        switch (t)
        {
            case CardType.Question: return question;
            case CardType.Sequence: return sequence;
            case CardType.Epi: return epi;
            case CardType.Wire: return wire;
            case CardType.Hunt: return hunt;
            case CardType.Match: return match;
            case CardType.Lockout: return lockout;
            default: return 0;
        }
    }
}
