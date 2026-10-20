using System.Collections.Generic;
using UnityEngine;

/// <summary>Baralho com pilha de compra e descarte. Reembaralha o descarte quando acaba.</summary>
public class CardDeck
{
    readonly List<CardData> drawPile = new List<CardData>();
    readonly List<CardData> discardPile = new List<CardData>();

    public int Remaining { get { return drawPile.Count; } }

    public CardDeck(List<CardData> cards)
    {
        drawPile.AddRange(cards);
        Shuffle(drawPile);
    }

    /// <summary>Lê Assets/Resources/{path}.json (cartas e pares da memória).</summary>
    public static CardCollection LoadFromResources(string path)
    {
        var empty = new CardCollection { cards = new List<CardData>(), pairs = new List<PairData>() };
        var asset = Resources.Load<TextAsset>(path);
        if (asset == null)
        {
            Debug.LogError("Arquivo de cartas não encontrado em Resources/" + path + ".json");
            return empty;
        }
        var collection = JsonUtility.FromJson<CardCollection>(asset.text);
        if (collection == null)
        {
            Debug.LogError("JSON de cartas inválido.");
            return empty;
        }
        if (collection.cards == null) collection.cards = new List<CardData>();
        if (collection.pairs == null) collection.pairs = new List<PairData>();
        return collection;
    }

    public CardData Draw()
    {
        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0) return null;
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            Shuffle(drawPile);
        }
        var card = drawPile[drawPile.Count - 1];
        drawPile.RemoveAt(drawPile.Count - 1);
        discardPile.Add(card);
        return card;
    }

    public static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp = list[i]; list[i] = list[j]; list[j] = tmp;
        }
    }
}
