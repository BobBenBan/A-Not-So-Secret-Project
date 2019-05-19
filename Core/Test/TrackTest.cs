using System;
using System.Collections.Generic;
using MusicMachine.Programs;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace MusicMachine.Test
{
[TestFixture]
public class TrackTest
{
    [SetUp]
    public static void Setup()
    {
        var random = new Randomizer(123);
        for (var i = 0; i < 3000; i++)
        {
            var key  = random.NextShort(1500);
            var item = random.GetString(3, "ABCDEF");
            if (!DenseKnown.ContainsKey(key))
                DenseKnown.Add(key, new List<string>());
            DenseKnown[key].Add(item);
            DenseTrack.Add(key, item);
        }

        for (var i = 0; i < 3000; i++)
        {
            var key  = random.NextShort(6000);
            var item = random.GetString(3, "ABCDEF");
            if (!SparseKnown.ContainsKey(key))
                SparseKnown.Add(key, new List<string>());
            SparseKnown[key].Add(item);
            SparseTrack.Add(key, item);
        }
    }
    private static readonly Track<string> DenseTrack = new Track<string>();
    private static readonly Dictionary<long, List<string>> DenseKnown = new Dictionary<long, List<string>>();
    private static readonly Track<string> SparseTrack = new Track<string>();
    private static readonly Dictionary<long, List<string>> SparseKnown = new Dictionary<long, List<string>>();
    private static void DoIterateTest(Track<string> track, Dictionary<long, List<string>> known)
    {
        foreach (var el in track.Elements)
        {
            Assert.IsTrue(known.ContainsKey(el.Key));
            Assert.AreEqual(known[el.Key], el.Value);
        }

        foreach (var el in track.EventPairs)
        {
            Assert.IsTrue(known.ContainsKey(el.Key));
            Assert.IsTrue(known[el.Key].Contains(el.Value));
        }

        known = new Dictionary<long, List<string>>(known);
        var random = new Randomizer(234);

        for (var j = 0; j < 500; j++)
        {
            var start = 30 + random.NextShort(400);
            var end   = start * 5 + random.NextShort(3000);
            var step  = 1 + random.NextShort(30);
            var cur   = start;
            var cnt   = 0;
            using (var pairs = track.IterateTrackNullSep(start, end, i => i + step).GetEnumerator())
            {
                while (pairs.MoveNext())
                {
                    var from = Math.Max(start, cur - step + 1);
                    var to   = Math.Min(end, cur);
                    while (pairs.Current.HasValue)
                    {
                        var pair = pairs.Current.Value;
                        Assert.That(pair.Key, Is.InRange(from, to));
                        Assert.IsTrue(known.ContainsKey(pair.Key));
                        Assert.AreEqual(known[pair.Key], pair.Value);

                        pairs.MoveNext();
                    }
                    cur += step;
                    if (cnt++ > 6000)
                        break;
                }
            }
        }
    }
    [Test]
    public static void OverFlowTest()
    {
        var random    = new Randomizer(345);
        var willThrow = false;

        long ThrowOrNot(long i)
        {
            if (random.NextShort(6000) >= 0)
                return i + 1 + random.NextShort(50);
            willThrow = true;
            return i;
        }

        try
        {
            foreach (var el in DenseTrack.IterateTrackSingleLists(0, 1233456, ThrowOrNot))
            {
            }
        }
        catch (OverflowException)
        {
            Assert.True(willThrow);
        }
    }
    [Test]
    public static void TestCopyAndTrim()
    {
        var randomizer = new Randomizer(123);
        var toRemove   = new List<long>();
        var cnt        = SparseTrack.Count;
        foreach (var time in SparseTrack.Times)
        {
            if (randomizer.NextShort(10) >= 1)
                continue;
            toRemove.Add(time);
            cnt--;
        }

        var newTrack = new Track<string>();
        newTrack.AddRange(SparseTrack.Elements);

        foreach (var i in toRemove)
        {
            Console.WriteLine($"Removing {i}");
            newTrack.RemoveAt(i);
        }

        Assert.AreEqual(cnt, newTrack.Count);
    }
    [Test]
    public static void TestEnumerators()
    {
        DoIterateTest(SparseTrack, SparseKnown);
        DoIterateTest(DenseTrack,  DenseKnown);
    }
}
}