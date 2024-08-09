using System;
using System.Collections.Generic;
using GifSupport.Data;
using UnityEngine;

namespace GifSupport.CstiGif;

[Serializable]
public class CardDataGif : ScriptableObject
{
    public static readonly Dictionary<CardData, CardDataGif> CardDict = new();

    public CardData Card;

    public GifPlaySet CardGif;

    public GifPlaySet CardBackgroundGif;

    public GifPlaySet CookingGif;

    public GifPlaySet DefaultLiquidGif;

    public LiquidSet[] LiquidGifs;

    [Serializable]
    public class LiquidSet
    {
        public int Index;

        public GifPlaySet LiquidGif;
    }

    static CardDataGif()
    {
        Loader.LoadCompleteEvent += OnLoadComplete;
    }

    private static void OnLoadComplete()
    {
        CardDict.Clear();

        var data = Database.GetData<CardDataGif>().Values;
        foreach (var obj in data)
        {
            var card = obj.Card;
            if (card is null) continue;
            if (CardDict.ContainsKey(card))
            {
                Plugin.Log.LogWarning($"Card {card.name} has multiple CardDataGif binding.");
                continue;
            }

            CardDict[obj.Card] = obj;
            obj.MapLiquidSets();
        }
    }

    private void MapLiquidSets()
    {
        if (LiquidGifs?.Length is null or < 1) return;

        var liquids = Card.LiquidImages;
        if (liquids?.Length is null or < 1) return;

        var map = new LiquidSet[liquids.Length];

        foreach (var set in LiquidGifs)
        {
            if (set is null || set.Index >= map.Length) continue;
            map[set.Index] = set;
        }
    }

    public static void SetCardGif(CardGraphics graphics)
    {
        var card = graphics?.CardLogic;
        if (!card?.CardModel) return;
        if (!CardDict.TryGetValue(card.CardModel, out var cardGif)) return;

        cardGif.CardBackgroundGif?.Apply(graphics.CardBG);
        cardGif.GetCurrentSet(graphics)?.Apply(graphics.CardImage);
    }

    private GifPlaySet GetCurrentSet(CardGraphics graphics)
    {
        var card = graphics.CardLogic;

        if (card.IsCooking())
        {
            if (CookingGif is not null) return CookingGif;
            if (graphics.CookingSprite) return null;
        }

        if (!card.ContainedLiquid) return CardGif;

        var liquid = card.ContainedLiquidModel;
        if (!liquid) return CardGif;

        var liquids = card.CardModel.LiquidImages;
        if (liquids?.Length is not > 0) return CardGif;

        var index = 0;
        var tag = false;
        for (; index < liquids.Length; index++)
        {
            var visual = liquids[index];
            if (visual is null || !visual.UseSprite(liquid)) continue;

            tag = true;
            break;
        }

        if (!tag) return DefaultLiquidGif ?? CardGif;
        if (LiquidGifs?.Length is not > 0) return null;

        var set = LiquidGifs[index];
        if (set is null || set.Index != index || set.LiquidGif is null) return null;
        return set.LiquidGif;
    }
}