using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using MusicMachine.Util;
using GDObject = Godot.Object;

namespace MusicMachine.ThirdParty.Midi
{
public class Bank : Wrapper<GDObject>
{
    private static readonly GDScript BankGd = GD.Load<GDScript>(GdLocations.Location + "Bank.gd");
    private static readonly GDScript SoundFontGd = GD.Load<GDScript>(GdLocations.Location + "SoundFont.gd");

    private readonly System.Collections.Generic.Dictionary<int, Preset> _presetCache =
        new System.Collections.Generic.Dictionary<int, Preset>();

    private readonly object _sf;

    public Bank(string soundFontFile, Array<int> usedProgramNumbers)
    {
        if (soundFontFile == null)
            throw new ArgumentNullException(nameof(soundFontFile));
        if (usedProgramNumbers == null)
            throw new ArgumentNullException(nameof(usedProgramNumbers));

        var soundFontReader = SoundFontGd.New();
        _sf = soundFontReader.Call("read_file", soundFontFile)
           ?? throw new FileNotFoundException("Cannot find synth file.");
        UpdateUsedProgNums(usedProgramNumbers);
    }

    public void UpdateUsedProgNums(Array<int> usedProgramNumbers)
    {
        if (usedProgramNumbers == null)
            throw new ArgumentNullException(nameof(usedProgramNumbers));
        var bank = BankGd.New();
        bank.Call("init");
        bank.Call("read_soundfont", _sf, usedProgramNumbers);
        Self = bank;
    }

    public Preset GetPreset(byte program, ushort bank)
    {
        var val = (bank << 7) | program;
        if (!_presetCache.TryGetValue(val, out var preset))
            _presetCache[val] = preset = new Preset((Dictionary) Self.Call("get_preset", program, bank));
        return preset;
    }
}

public class Preset : Wrapper<Dictionary>
{
    private readonly IReadOnlyList<Instrument> _instruments;

    internal Preset(Dictionary self)
    {
        if (self == null)
        {
            _instruments = new Instrument[128];
            return;
        }
        Self = self;
        var instruments = new List<Instrument>();
        _instruments = instruments;
        var gdInstruments = Self["instruments"];
        instruments.AddRange(
            from object instrument in (IEnumerable) gdInstruments
            select instrument != null ? new Instrument((Dictionary) instrument) : null);
    }

    public Instrument this[byte keyNumber] => _instruments[keyNumber];
}

public class Instrument : Wrapper<Dictionary>
{
    internal Instrument(Dictionary self)
    {
        Self         = self;
        MixRate      = (float) Self["mix_rate"];
        Stream       = (AudioStreamSample) Self["stream"];
        AdsState     = State.Create(Self["ads_state"]);
        ReleaseState = State.Create(Self["release_state"]);
    }

    public float MixRate { get; }

    public AudioStreamSample Stream { get; }

    public State[] AdsState { get; }

    public State[] ReleaseState { get; }
}
}