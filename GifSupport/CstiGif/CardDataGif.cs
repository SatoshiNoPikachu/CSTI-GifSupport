using System;
using System.Collections.Generic;
using System.Linq;
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

    public DurabilitySet[] DurabilitySets;

    [Serializable]
    public class LiquidSet
    {
        public int Index;

        public GifPlaySet LiquidGif;
    }

    [Serializable]
    public class DurabilitySet
    {
        public GifPlaySet Gif;

        public DurabilityCondition[] Conditions;

        public bool Check(InGameCardBase card)
        {
            if (!card) return false;
            return Conditions is not (null or { Length: < 1 }) &&
                   Conditions.All(condition => condition?.Check(card) is true);
        }
    }

    [Serializable]
    public class DurabilityCondition
    {
        public DurabilitiesTypes DurabilityType;

        public float MinValue;

        public float MaxValue;

        public bool Check(InGameCardBase card)
        {
            if (!card) return false;

            var value = GetCurrentValue(card);
            if (value < 0) return false;

            return value >= MinValue && value <= MaxValue;
        }

        private float GetCurrentValue(InGameCardBase card)
        {
            return DurabilityType switch
            {
                DurabilitiesTypes.Spoilage => card.CurrentSpoilage,
                DurabilitiesTypes.Usage => card.CurrentUsageDurability,
                DurabilitiesTypes.Fuel => card.CurrentFuel,
                DurabilitiesTypes.Progress => card.CurrentProgress,
                DurabilitiesTypes.Special1 => card.CurrentSpecial1,
                DurabilitiesTypes.Special2 => card.CurrentSpecial2,
                DurabilitiesTypes.Special3 => card.CurrentSpecial3,
                DurabilitiesTypes.Special4 => card.CurrentSpecial4,
                DurabilitiesTypes.Liquid => -1,
                _ => -1
            };
        }
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
        if (!CardDict.TryGetValue(card.CardModel, out var cardGif))
        {
            GifPlaySet.Clear(graphics.CardBG);
            GifPlaySet.Clear(graphics.CardImage);
            return;
        }

        var gif = cardGif.CardBackgroundGif;
        if (gif is not null) gif.Apply(graphics.CardBG);
        else GifPlaySet.Clear(graphics.CardBG);

        gif = cardGif.GetCurrentSet(graphics);
        if (gif is not null) gif.Apply(graphics.CardImage);
        else GifPlaySet.Clear(graphics.CardImage);
    }

    private GifPlaySet GetCurrentSet(CardGraphics graphics)
    {
        var card = graphics.CardLogic;

        if (DurabilitySets is not null)
        {
            foreach (var set in DurabilitySets)
            {
                if (set?.Gif is null || !set.Check(card)) continue;
                return set.Gif;
            }
        }

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

        var liquidSet = LiquidGifs[index];
        if (liquidSet is null || liquidSet.Index != index || liquidSet.LiquidGif is null) return null;
        return liquidSet.LiquidGif;
    }
}