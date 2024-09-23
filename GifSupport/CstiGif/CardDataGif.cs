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

    public ConditionSet[] ConditionSets;

    [Serializable]
    public class LiquidSet
    {
        public int Index;

        public GifPlaySet LiquidGif;
    }

    [Serializable]
    public class ConditionSet
    {
        public GifPlaySet Gif;

        public DurabilityCondition[] DurabilityConditions;

        public StatCondition[] StatConditions;

        public bool Check(InGameCardBase card)
        {
            if (!card) return false;
            return CheckDurability(card) && CheckStat();
        }

        private bool CheckDurability(InGameCardBase card)
        {
            return DurabilityConditions?.All(condition => condition?.Check(card) is true) is true or null;
        }

        private bool CheckStat()
        {
            return StatConditions?.All(condition => condition?.Check() is true) is true or null;
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

    [Serializable]
    public class StatCondition
    {
        public GameStat Stat;

        public float MinValue;

        public float MaxValue;

        public bool Check()
        {
            if (Stat is null) return false;
            if (!GameManager.Instance.StatsDict.TryGetValue(Stat, out var stat)) return false;

            var value = stat.CurrentValue(GameManager.Instance.NotInBase);
            return value >= MinValue && value <= MaxValue;
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
        if (gif?.Gif is not null) gif.Apply(graphics.CardImage);
        else GifPlaySet.Clear(graphics.CardImage);
    }

    public static void SetCarDesc(InGameCardBase card, ref string description, bool ignoreLiquid)
    {
        if (!card?.CardModel) return;
        if (!CardDict.TryGetValue(card.CardModel, out var cardGif)) return;

        var graphics = card.CardVisuals;
        if (!graphics) return;

        var set = cardGif.GetCurrentSet(graphics, ignoreLiquid);
        if (set is null) return;

        var desc = set.Description.ToString();
        if (desc == "") return;
        description = desc;
    }

    private GifPlaySet GetCurrentSet(CardGraphics graphics, bool ignoreLiquid = false)
    {
        var card = graphics.CardLogic;

        if (ConditionSets is not null)
        {
            foreach (var set in ConditionSets)
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

        if (ignoreLiquid || !card.ContainedLiquid) return CardGif;

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