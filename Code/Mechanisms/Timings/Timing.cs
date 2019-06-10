using Godot;

namespace MusicMachine.Mechanisms.Timings
{
/// <summary>
///     I am not concurrent. Be cautioned.
/// </summary>
public class Timing : Object
//feel free to extend!!!!!!!!!!
{
    public enum TimingStatus { Unstarted, InProgress, Done }

    private long? _endTime;
    private long? _startTime;
    private TimingRecorder _timingRecorder;

    public TimingStatus Status =>
        _startTime == null ? TimingStatus.Unstarted :
        _endTime == null   ? TimingStatus.InProgress : TimingStatus.Done;

    internal TimingRecorder TimingRecorder
    {
        set
        {
            CancelTiming();
            _timingRecorder = value;
        }
        private get { return _timingRecorder; }
    }

    public long? ElapsedTicks => _endTime - _startTime;

    public long? ElapsedMicros => (ElapsedTicks + 5) / 10;

    protected virtual void OnStart()
    {
    }

    protected virtual void OnCancel()
    {
    }

    protected virtual void OnReset()
    {
    }

    protected virtual void OnEnd()
    {
    }

    public bool StartTiming(bool cancel, bool restart)
    {
        var status = Status;
        if (TimingRecorder == null
         || !(status == TimingStatus.Unstarted
           || cancel && status == TimingStatus.InProgress
           || restart && status == TimingStatus.Done))
            return false;

        if (status != TimingStatus.Unstarted) ResetTiming();
        _startTime = TimingRecorder.CurTicks;
        OnStart();
        TimingRecorder.NotifyStart();
        return true;
    }

    public bool CancelTiming()
    {
        if (Status != TimingStatus.InProgress) return false;
        _startTime = null;
        _endTime   = null;
        OnCancel();
        TimingRecorder.NotifyCancel();
        return true;
    }

    public bool ResetTiming()
    {
        if (Status == TimingStatus.Unstarted) return false;
        if (Status == TimingStatus.InProgress) return CancelTiming();
        _startTime = null;
        _endTime   = null;
        OnReset();
        TimingRecorder.NotifyReset();
        return true;
    }

    public bool EndTiming()
    {
        if (Status != TimingStatus.InProgress) return false;
        _endTime = TimingRecorder.CurTicks;
        OnEnd();
        TimingRecorder.NotifyEnd();
        return true;
    }
}
}