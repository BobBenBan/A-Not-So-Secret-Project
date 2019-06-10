using System.Collections.Generic;
using MusicMachine.Mechanisms.Glowing;
using MusicMachine.Scenes.Mechanisms.Glowing;
using MusicMachine.Util.Maths;

namespace MusicMachine.Scenes.Objects.Drums
{
public class Drums : SimpleGlowingArray
{
    private readonly Dictionary<SBN, SBN> _noteMapper = new Dictionary<SBN, SBN>();
    private int _realNoteIdx;

    public override IContainsGlowing GetGlowingForNote(SBN note)
    {
        if (!_noteMapper.TryGetValue(note, out var realNote))
        {
            if (_realNoteIdx >= NumObjects) return null;
            realNote = _noteMapper[note] = (SBN) _realNoteIdx++;
        }
        return TheGlows[realNote];
    }
}
}