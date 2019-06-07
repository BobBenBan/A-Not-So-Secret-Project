namespace MusicMachine.Scenes.Functional.Timing
{
public class Timing : Godot.Object
//feel free to extend!!!!!!!!!!
{
    public enum TimingStatus { Unstarted, InProgress, Done }

    public TimingStatus Status =>
        _startTime == null ? TimingStatus.Unstarted :
        _endTime == null   ? TimingStatus.InProgress :
                             TimingStatus.Done;

    private long? _startTime;
    private long? _endTime;
    private TimingRecorder _timingRecorder;

    internal TimingRecorder TimingRecorder
    {
        set
        {
            CancelTiming(false);
            _timingRecorder = value;
        }
        private get { return _timingRecorder; }
    }

    public long? ElapsedTicks => _endTime - _startTime;
    public long? ElapsedMicros => (ElapsedTicks + 5) / 10;

    protected virtual void OnStart()
    {
    }

    protected virtual void OnCancel(bool isDone)
    {
    }

    protected virtual void OnEnd()
    {
    }

    public bool StartTiming(bool cancel, bool restart)
    {
        var status = Status;
        if (TimingRecorder == null
         || status != TimingStatus.Unstarted && !(cancel && status == TimingStatus.InProgress) && !restart /*&& true*/)
            return false;
        if (status != TimingStatus.Unstarted)
        {
            _startTime = null;
            _endTime = null;
            OnCancel(status == TimingStatus.Done);
        }
        _startTime = TimingRecorder.CurTicks;
        OnStart();
        return true;
    }

    public bool CancelTiming(bool ifDone)
    {
        var status = Status;
        if (status == TimingStatus.Unstarted || !ifDone && status == TimingStatus.Done) return false;
        _startTime = null;
        _endTime = null;
        OnCancel(status == TimingStatus.Done);
        return true;
    }

    public bool EndTiming()
    {
        if (Status != TimingStatus.InProgress) return false;
        _endTime = TimingRecorder.CurTicks;
        OnEnd();
        return true;
    }
}
}