using System.Collections.Generic;
using UnityEngine;

public static class GameResultEvaluator
{
    public enum Tier { Bad, Mid, Good }

    public struct Dimension
    {
        public string Label;
        public int    Score;
        public string Comment;
    }

    public class Result
    {
        public int             FinalScore;
        public Tier            Tier;
        public string          TierLabel;
        public Color           TierColor;
        public string          Headline;
        public string          Summary;
        public List<Dimension> Dimensions = new List<Dimension>();
    }

    private const int GoodThreshold = 70;
    private const int MidThreshold  = 40;
    private const int HighBand      = 70;
    private const int LowBand       = 40;

    private static readonly Color GoodColor = new Color(0.18f, 0.80f, 0.44f);
    private static readonly Color MidColor  = new Color(0.96f, 0.75f, 0.06f);
    private static readonly Color BadColor  = new Color(0.92f, 0.25f, 0.20f);

    public static Result Evaluate(int health, int stress, int money, int idealMoney)
    {
        int mental     = Mathf.Clamp(100 - stress, 0, 100);
        int moneyScore = Mathf.RoundToInt(Mathf.Clamp01((float)money / Mathf.Max(idealMoney, 1)) * 100f);

        int finalScore = Mathf.RoundToInt((health + mental + moneyScore) / 3f);

        var result = new Result { FinalScore = finalScore };

        if (finalScore >= GoodThreshold)
        {
            result.Tier      = Tier.Good;
            result.TierLabel = "BIEN";
            result.TierColor = GoodColor;
            result.Headline  = "¡Muy bien! Llevaste una vida bastante equilibrada.";
        }
        else if (finalScore >= MidThreshold)
        {
            result.Tier      = Tier.Mid;
            result.TierLabel = "INTERMEDIO";
            result.TierColor = MidColor;
            result.Headline  = "Te fue más o menos: cuidaste algunas cosas y descuidaste otras.";
        }
        else
        {
            result.Tier      = Tier.Bad;
            result.TierLabel = "MAL";
            result.TierColor = BadColor;
            result.Headline  = "Te costó mantener el equilibrio. Varios aspectos quedaron descuidados.";
        }

        result.Dimensions.Add(new Dimension
        {
            Label   = "Salud física",
            Score   = health,
            Comment = Band(health,
                "Cuidaste muy bien tu salud física.",
                "Tu salud física quedó aceptable, pero podías cuidarla más.",
                "Descuidaste tu salud física.")
        });

        result.Dimensions.Add(new Dimension
        {
            Label   = "Salud mental",
            Score   = mental,
            Comment = Band(mental,
                "Mantuviste la mente tranquila y con poco estrés.",
                "Manejaste el estrés, aunque acumulaste algo de tensión.",
                "Terminaste muy estresado/a: tu salud mental sufrió.")
        });

        result.Dimensions.Add(new Dimension
        {
            Label   = "Dinero",
            Score   = moneyScore,
            Comment = Band(moneyScore,
                $"Terminaste con ${money}. Manejaste muy bien tu dinero.",
                $"Terminaste con ${money}. Tu dinero quedó justo.",
                $"Terminaste con ${money}. Te quedaste con muy poco dinero.")
        });

        result.Summary = BuildSummary(result.Dimensions);
        return result;
    }

    private static string Band(int score, string high, string mid, string low)
    {
        if (score >= HighBand) return high;
        if (score >= LowBand)  return mid;
        return low;
    }

    private static string BuildSummary(List<Dimension> dims)
    {
        if (dims == null || dims.Count == 0) return string.Empty;

        Dimension best  = dims[0];
        Dimension worst = dims[0];
        foreach (var d in dims)
        {
            if (d.Score > best.Score)  best  = d;
            if (d.Score < worst.Score) worst = d;
        }

        if (worst.Score >= HighBand)
            return "Mantuviste un gran equilibrio en todos los aspectos. ¡Excelente trabajo!";

        if (best.Score < LowBand)
            return "Casi todos los aspectos quedaron descuidados. Intentá repartir mejor tu tiempo y tus recursos.";

        if (best.Label == worst.Label)
            return $"Tu desempeño fue parejo, rondando los {best.Score} puntos.";

        return $"Tu punto más fuerte fue «{best.Label}» ({best.Score}), " +
               $"y lo que más descuidaste fue «{worst.Label}» ({worst.Score}).";
    }
}
