using Rationals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnitConversion;

internal enum Units
{
    Mm, Cm, Dm, M, Km
}

internal class Program
{
    private static readonly int[] RelevantPrimes = [3, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503, 509, 521, 523, 541];
    private static void Main(string[] args)
    {
        try
        {
            // 0=mm, 1=cm, 2=dm, 3=km
            var p = from power in Enumerable.Range(1, 3)
                    from highUnit in Enum.GetValues(typeof(Units)).Cast<Units>()
                    from lowUnit in Enum.GetValues(typeof(Units)).Cast<Units>()
                    where lowUnit < highUnit
                    from number in Enumerable.Range(1, 100).Select(x => 10 * x)
                    from denominator in new[] { 1, 2, 4, 5, 8, 15, 20 }
                    from numerator in Enumerable.Range(1, 20)
                    where numerator < denominator
                    let r = (Rational)numerator / denominator
                    where r.CanonicalForm.Denominator == denominator
                    where !IsPeriodic(number, numerator, denominator)
                    let answer = GetAnswer(number, lowUnit, highUnit, power, numerator, denominator)
                    where answer < 100_000
                    select (answer, number, lowUnit, highUnit, power, numerator, denominator);

            var result = p.ToList();
            Shuffle(result);
            foreach (var (answer, number, lowUnit, highUnit, power, numerator, denominator) in result.Take(20))
            {
                Console.WriteLine($"How many {lowUnit}^{power} is {numerator}/{denominator} of {number} {highUnit}^{power} ({answer})");
            }

        }
        catch (Exception ex)
        {
            var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
            var progname = Path.GetFileNameWithoutExtension(fullname);
            Console.Error.WriteLine($"{progname} Error: {ex.Message}");
        }
    }

    private static bool IsPeriodic(int number, int numerator, int denominator)
    {
        var r = ((Rational)number * numerator / denominator).CanonicalForm;

        return RelevantPrimes.Any(x => r.Denominator % x == 0) ? true : false;
    }

    public static void Shuffle<T>(IList<T> list)
    {
        var rnd = new Random();

        for (int i = list.Count() - 1; i > 0; i--)
        {
            // random from zero to i:
            var j = rnd.Next(i + 1);

            // swap:
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private static decimal GetAnswer(int number, Units lowUnit, Units highUnit, int power, int numerator, int denominator)
    {
        long factor = (highUnit, lowUnit) switch
        {
            (Units.Cm, Units.Mm) => 10,
            (Units.Dm, Units.Mm) => 100,
            (Units.M, Units.Mm) => 1000,
            (Units.Km, Units.Mm) => 1_000_000,

            (Units.Dm, Units.Cm) => 10,
            (Units.M, Units.Cm) => 100,
            (Units.Km, Units.Cm) => 100_000,

            (Units.M, Units.Dm) => 10,
            (Units.Km, Units.Dm) => 10_000,

            (Units.Km, Units.M) => 1000,
            _ => throw new InvalidOperationException($"Cannot convert {highUnit} to {lowUnit}")
        };

        long powerFactor = power switch
        {
            1 => factor,
            2 => factor * factor,
            3 => factor * factor * factor,
            _ => throw new InvalidOperationException($"power of {power} is invalid")
        };

        var result = (decimal)number * powerFactor * numerator / denominator;
        return result;
    }
}
