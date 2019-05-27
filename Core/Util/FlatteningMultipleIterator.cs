using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MusicMachine
{
/// <summary>
///     Combines a series of Enumerables of Enumerables, into a single Enumerable of Enumerables.
///     I hope that make sense.
///     The IEnumerables returned by this are TEMPORARY. Please note.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public struct FlatteningMultipleIterator<TEvent> : IEnumerable<IEnumerable<TEvent>>
{
    private readonly IEnumerable<IEnumerable<IEnumerable<TEvent>>> enumerofEnumerOfEnumerables; //not a mouthful at all.

    public FlatteningMultipleIterator(IEnumerable<IEnumerable<IEnumerable<TEvent>>> enumerables)
    {
        enumerofEnumerOfEnumerables = enumerables;
    }

    public IEnumerator<IEnumerable<TEvent>> GetEnumerator()
    {
        var enumerators = enumerofEnumerOfEnumerables.Select(x => x.GetEnumerator()).ToList();

        var curBatch = new List<TEvent>();
        while (true)
        {
            for (var index = enumerators.Count - 1; index >= 0; index--)
            {
                var enumerator = enumerators[index];
                if (!enumerator.MoveNext())
                {
                    enumerators.RemoveAt(index);
                    continue;
                }
                if (enumerator.Current != null)
                    curBatch.AddRange(enumerator.Current);
            }
            if (enumerators.Count != 0)
            {
                yield return curBatch;
                curBatch.Clear();
            }
            else
                break;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
}