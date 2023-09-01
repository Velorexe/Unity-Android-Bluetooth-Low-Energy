namespace Android.BLE
{
    public struct BleTask
    {
        public string MethodDefinition;

        public object[] Parameters;

        public BleTask(string methodDefinition, params object[] parameters)
        {
            MethodDefinition = methodDefinition;
            Parameters = parameters;
        }
    }
}
