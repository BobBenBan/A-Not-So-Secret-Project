using System.Collections.Generic;
using System.Linq;
using MusicMachine.Programs;
using MusicMachine.Util;
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
                DenseKnown[key] = new List<string>();
            DenseKnown[key].Add(item);
            DenseTrack.Add(key, item);
        }

        for (var i = 0; i < 3000; i++)
        {
            var key  = random.NextShort(6000);
            var item = random.GetString(3, "ABCDEF");
            if (!SparseKnown.ContainsKey(key))
                SparseKnown[key] = new List<string>();
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
        known = new Dictionary<long, List<string>>(known);
        var random  = new Randomizer(234);
        var curList = new List<Pair<long, IReadOnlyList<string>>>();
        for (var j = 0; j < 200; j++)
        {
            var start = -100 + random.NextShort(400);

            var stepper = track.GetStepper(start);
            var cur     = start;
            var cnt     = 0;
            while (true)
            {
                var next = cur + random.NextShort(70);
                if (!stepper.StepToInclusive(next))
                {
                    Assert.Greater(cur, track.Times.Last());
                    break;
                }
                stepper.CopyCurrentTo(curList);
                for (var i = cur; i <= next; i++)
                {
                    var knownHasKey = known.ContainsKey(i);
                    var trackIndex  = curList.FindIndex(x => x.First == i);
                    Assert.AreEqual(knownHasKey, trackIndex >= 0);
                    if (!knownHasKey)
                        continue;
                    Assert.AreEqual(known[i], curList[trackIndex].Second);
                    curList.RemoveAt(trackIndex);
                }
                Assert.IsEmpty(curList);
                cur = next + 1;
                if (cnt++ > 8000)
                    break;
            }
        }
    }

    [Test]
    public static void TestStepper()
    {
        DoIterateTest(SparseTrack, SparseKnown);
        DoIterateTest(DenseTrack,  DenseKnown);
    }
}
}