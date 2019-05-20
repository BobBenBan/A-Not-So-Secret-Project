using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using GDObject = Godot.Object;

namespace MusicMachine.ThirdParty.Midi
{
public class Bank : Wrapper<GDObject>
{
    private static readonly GDScript BankGd = GD.Load<GDScript>(Midi.Location + "Bank.gd");
    private static readonly GDScript SoundFontGd = GD.Load<GDScript>(Midi.Location + "SoundFont.gd");
    public Bank(string soundFontFile, Array<int> usedProgramNumbers)
    {
        if (soundFontFile == null)
            throw new ArgumentNullException(nameof(soundFontFile));
        if (usedProgramNumbers == null)
            throw new ArgumentNullException(nameof(usedProgramNumbers));

        var bank = BankGd.New();
        bank.Call("init");
        var soundFontReader = SoundFontGd.New();
        var sf              = soundFontReader.Call("read_file", soundFontFile);
        bank.Call("read_soundfont", sf, usedProgramNumbers);
        Self = bank;
    }

    private System.Collections.Generic.Dictionary<int, Preset> presetCache =
        new System.Collections.Generic.Dictionary<int, Preset>();

    public Preset GetPreset(byte program, ushort bank)
    {
        var val = (bank << 7) | program;
        if (!presetCache.TryGetValue(val, out var preset))
            presetCache.Add(val, preset = new Preset((Dictionary) Self.Call("get_preset", program, bank)));
        Self.Call("get_preset", program, bank);
        return preset;
    }
}

public class Preset : Wrapper<Dictionary>
{
    private readonly List<Instrument> _instruments = new List<Instrument>();
    internal Preset(Dictionary self)
    {
        Self = self;
        var gdInstruments = Self["instruments"];
        foreach (var instrument in (IEnumerable) gdInstruments)
        {
            _instruments.Add(instrument != null ? new Instrument((Dictionary) instrument) : null);
        }
    }

    public Instrument this[byte keyNumber] => _instruments[keyNumber];
}

public class Instrument : Wrapper<Dictionary>
{
    internal Instrument(Dictionary self)
    {
        Self = self;
        MixRate = (float) Self["mix_rate"];
        Stream = (AudioStreamSample) Self["stream"];
        AdsState = State.Create(Self["ads_state"]);
        ReleaseState = State.Create(Self["release_state"]);
    }

    public float MixRate { get; }

    public AudioStreamSample Stream { get; }

    public State[] AdsState { get; }

    public State[] ReleaseState { get; }
}
}