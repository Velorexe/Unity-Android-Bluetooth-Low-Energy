namespace Android.BLE.Commands
{
    public abstract class BleCommand
    {
        public float Timeout { get => _timeout; }
        protected float _timeout = 5f;

        public readonly bool RunParallel = false;
        public readonly bool RunContiniously = false;

        public BleCommand(bool runParallel = false, bool runContiniously = false)
        {
            RunParallel = runParallel;
            RunContiniously = runContiniously;
        }

        public abstract void Start(BleManager callBack);

        public virtual void End(BleManager callBack) { }

        public virtual void EndOnTimeout(BleManager callBack) => End(callBack);

        public virtual bool CommandReceived(BleObject obj)
        {
            return false;
        }
    }
}