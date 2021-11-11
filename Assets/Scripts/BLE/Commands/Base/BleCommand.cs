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

        public abstract void Start();

        public virtual void End() { }

        public virtual void EndOnTimeout() => End();

        public virtual bool CommandReceived(BleObject obj)
        {
            return false;
        }
    }
}