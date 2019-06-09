using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace MusicMachine.Test
{
[TestFixture]
public class PseudoScratch
{
    public static void EnumName()
    {
        var random = new Randomizer(123);
        for (var i = 0; i < 20; i++)
            Console.WriteLine(random.NextEnum<E>().ToString());
    }

    private static IEnumerable<string> ForeverEnumerator()
    {
        try
        {
            while (true)
                yield return "hi";
        }
        finally
        {
            Console.WriteLine("Finally entered");
            throw new Exception();
        }
    }

    public static void EnumCast()
    {
        var e = (E) 9;
        Console.Write(e.ToString());
    }

    private enum E
    {
        EnumA = 1,
        BEe = 2,
        Cee = 4,
        Dirt = 5,
        EEt = 6
    }

    [Test]
    public static void EnumFinallyTest()
    {
        Assert.Throws<Exception>(
            () =>
            {
                var enumerator = ForeverEnumerator().GetEnumerator();
                for (var i = 0; i < 100; i++)
                {
                    enumerator.MoveNext();
                    Console.Write(enumerator.Current);
                }
                Console.WriteLine();
                Console.WriteLine("Disposing...");
                enumerator.Dispose();
            });
    }
}
}