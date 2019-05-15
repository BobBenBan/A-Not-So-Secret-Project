using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace MusicMachine.Test
{
public class PseudoScratch
{
    enum E
    {
        Bleh,
        ksadjf,
        KHDKJSDF,
        IJDFISDKJF,
        HelpUs
    }

    [Test]
    public static void EnumName()
    {
        var random = new Randomizer(123);
        for (var i = 0; i < 20; i++)
        {
            Console.WriteLine(random.NextEnum<E>().ToString());
        }
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
            throw new TestException();
        }
    }

    [Test]
    public static void EnumFinallyTest()
    {
        Assert.Throws<TestException>(
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

    public static void Random()
    {
        var a = new SortedSet<int>();
    }
}
}