namespace MusicMachine.Scenes.Functional.Timing
{
public abstract class Timing
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
            Cancel(false);
            _timingRecorder = value;
        }
        private get { return _timingRecorder; }
    }

    public long? ElapsedTime => _endTime - _startTime;

    protected abstract void OnStart();

    protected abstract void OnCancel(bool isDone);

    protected abstract void OnEnd();

    public bool Start(bool cancel, bool restart)
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

    public bool Cancel(bool ifDone)
    {
        var status = Status;
        if (status == TimingStatus.Unstarted || !ifDone && status == TimingStatus.Done) return false;
        _startTime = null;
        _endTime = null;
        OnCancel(status == TimingStatus.Done);
        return true;
    }

    public bool End()
    {
        if (Status != TimingStatus.InProgress) return false;
        _endTime = TimingRecorder.CurTicks;
        OnEnd();
        return true;
    }
}
}