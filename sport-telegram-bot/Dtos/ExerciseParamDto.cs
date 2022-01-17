using System;

namespace sport_telegram_bot.Dtos;

public sealed class ExerciseParamDto
{
    public int Repetitions { get; init; }
    public int Weight { get; init; }

    public static ExerciseParamDto Parse(string message)
    {
        try
        {
            var paramStrings = message.Split("-");
            return new ExerciseParamDto
            {
                Repetitions = int.Parse(paramStrings[0]),
                Weight = int.Parse(paramStrings[1])
            };

        }
        catch (Exception)
        {
            throw new Exception($"Message cant parse to {nameof(ExerciseParamDto)}");
        }
    }
}