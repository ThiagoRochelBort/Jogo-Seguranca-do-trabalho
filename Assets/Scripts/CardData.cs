using System;
using System.Collections.Generic;

public enum CardType { Question, Sequence, Epi, Event, Wire, Hunt, Match, Lockout }

/// <summary>
/// Uma carta do baralho. Todos os tipos usam a mesma classe (o JsonUtility
/// da Unity não suporta herança), e cada tipo usa só os campos que precisa.
/// </summary>
[Serializable]
public class CardData
{
    public string id;
    public string type;        // "question", "sequence", "epi" ou "event"
    public string nr;          // ex.: "NR-6"
    public string title;
    public string prompt;
    public string explanation; // mostrado depois da resposta

    // Pergunta
    public string[] options;
    public int correctIndex;

    // Sequência (os passos já na ORDEM CORRETA; o jogo embaralha)
    public string[] steps;

    // Seleção de EPI
    public string[] items;
    public int[] correctItems;

    // Evento
    public int healthDelta;
    public int auditDelta;

    // Dano ao errar (0 ou ausente = 1)
    public int damage;

    // Desafios práticos (ver "Desafios práticos" no GDD)
    public int maxMistakes;        // 0 ou ausente = padrão do GameManager
    public int seconds;            // só hunt; 0 = sem tempo

    // wire: EPI + ligar fios. links[i] = índice em right do fio left[i]
    public string[] epiItems;
    public int[] epiCorrect;
    public string[] left;
    public string[] right;
    public int[] links;
    public string epiWhy;
    public string linkWhy;
    // wire, visual (opcionais): ícone de cada EPI, cor de cada fio e o equipamento da direita
    public string[] epiIcons;
    public string[] wireColors;  // "preto", "azul", "marrom", "verde_amarelo"
    public string device;        // "tomada", "bomba", "luminaria", "painel"

    // hunt (P2b)
    public HuntItem[] huntItems;

    // match (P2b)
    public MatchPair[] matchPairs;
    public string[] extras;
    public string[] extraIcons;  // visual (opcional), mesmo tamanho de extras

    // lockout: usa steps (na ordem certa) + traps
    public TrapData[] traps;
    // lockout, visual (opcionais): ícone e efeito na cena de cada passo
    public string[] stepIcons;
    public string[] stepFx;      // avisar, desligar, bloquear, etiquetar, medir, aterrar, testar, documento, inspecionar, ancorar, isolar, entrar

    // hunt e lockout, visual (opcional): cena desenhada
    public string scene;         // hunt: oficina, telhado, obra; lockout: maquina, painel, altura, tanque

    public CardType Type
    {
        get
        {
            switch ((type ?? "").ToLowerInvariant())
            {
                case "sequence": return CardType.Sequence;
                case "epi": return CardType.Epi;
                case "event": return CardType.Event;
                case "wire": return CardType.Wire;
                case "hunt": return CardType.Hunt;
                case "match": return CardType.Match;
                case "lockout": return CardType.Lockout;
                default: return CardType.Question;
            }
        }
    }

    /// <summary>Confere se a carta tem os dados que o tipo dela precisa (evita erro no meio do desafio).</summary>
    public bool HasRequiredData()
    {
        switch (Type)
        {
            case CardType.Wire:
                if (epiItems == null || epiCorrect == null || left == null || right == null || links == null) return false;
                if (links.Length != left.Length) return false;
                var usedRight = new HashSet<int>();
                foreach (int r in links)
                {
                    if (r < 0 || r >= right.Length) return false;
                    if (!usedRight.Add(r)) return false; // dois fios no mesmo borne: impossível fechar
                }
                foreach (int e in epiCorrect) if (e < 0 || e >= epiItems.Length) return false;
                return left.Length > 0;
            case CardType.Lockout:
                return steps != null && steps.Length > 0;
            case CardType.Hunt:
                if (huntItems == null) return false;
                foreach (var h in huntItems) if (h.risk) return true;
                return false; // sem nenhum risco não tem o que achar
            case CardType.Match:
                return matchPairs != null && matchPairs.Length > 0;
            default:
                return true;
        }
    }
}

[Serializable]
public class HuntItem
{
    public string label;
    public string shape;   // "rect" ou "circle"
    public float x, y, w, h; // % da área da cena (0 a 100)
    public bool risk;
    public string why;
    public string icon;      // visual (opcional)
}

[Serializable]
public class MatchPair
{
    public string s;   // situação
    public string a;   // ação certa
    public string why;
    public string sIcon; // visual (opcional): ícone da situação
    public string aIcon; // visual (opcional): ícone da ação
}

[Serializable]
public class TrapData
{
    public string label;
    public string why;
    public string icon;      // visual (opcional)
}

/// <summary>
/// Um par do jogo da memória. "a" e "b" viram duas cartas na mesa.
/// kind "nr": a = número da norma, b = tema. kind "epi": a = risco, b = EPI.
/// </summary>
[Serializable]
public class PairData
{
    public string id;
    public string kind;
    public string a;
    public string b;
    public string explanation; // mostrado quando a equipe encontra o par

    public bool IsNr { get { return (kind ?? "").ToLowerInvariant() == "nr"; } }
}

/// <summary>Uma carta da mesa: metade de um par.</summary>
public class MemoryTile
{
    public PairData pair;
    public bool sideA;
    public bool matched;

    public string Tag { get { return pair.IsNr ? (sideA ? "NORMA" : "TEMA") : (sideA ? "RISCO" : "EPI"); } }
    public string Text { get { return sideA ? pair.a : pair.b; } }
}

[Serializable]
public class CardCollection
{
    public List<CardData> cards;
    public List<PairData> pairs;
}
