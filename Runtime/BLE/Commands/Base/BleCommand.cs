namespace Android.BLE.Commands
{
    /// <summary>
    /// The base class to interact with the Java library.
    /// </summary>
    public abstract class BleCommand
    {
        /// <summary>
        /// If queued as an active command, this will be it's maximum lifespan.
        /// </summary>
        public float Timeout { get => _timeout; }
        protected float _timeout = 5f;

        /// <summary>
        /// If <see langword="true"/>, the <see cref="BleCommand"/> will run parallel with 
        /// other commands. Helpful for commands such as <see cref="SubscribeToCharacteristic"/>.
        /// </summary>
        public bool RunParallel
        {
            get;
            protected set;
        }

        /// <summary>
        /// Base initialization of the <see cref="BleCommand"/>.
        /// </summary>
        public BleCommand(bool runParallel = false)
        {
            RunParallel = runParallel;
        }

        /// <summary>
        /// Starts the the execution of the <see cref="BleCommand"/>.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Ends the <see cref="BleCommand"/>, useful for unsubscribing from Characteristics 
        /// for commands such as <see cref="SubscribeToCharacteristic"/>.
        /// </summary>
        public virtual void End() { }

        /// <summary>
        /// Ends the <see cref="BleCommand"/> when time runs out, by default calls <see cref="End"/>.
        /// </summary>
        public virtual void EndOnTimeout() => End();

        /// <summary>
        /// The <see cref="BleManager"/> will send <see cref="BleObject"/> through this 
        /// method, which it can use to react accordingly.
        /// </summary>
        /// <param name="obj">The <see cref="BleObject"/> containing the raw information from the Java library.</param>
        /// <returns>Returns <see langword="true"/> if the <see cref="BleCommand"/> consumes the <see cref="BleObject"/>.</returns>
        public virtual bool CommandReceived(BleObject obj)
        {
            return false;
        }


    }
}