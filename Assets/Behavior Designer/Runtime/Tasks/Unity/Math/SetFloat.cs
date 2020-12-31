namespace BehaviorDesigner.Runtime.Tasks.Unity.Math
{
    [TaskCategory("Unity/Math")]
    [TaskDescription("Sets a float value")]
    public class SetFloat : Action
    {
        [Tooltip("The float value to set")]
        public SharedFloat floatValue;
        [Tooltip("The variable to store the result")]
        public SharedFloat storeResult;

        public override TaskStatus OnUpdate()
        {
            storeResult.Value = floatValue.Value;
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            if (floatValue != null)
                floatValue.Value = 0;
            if (storeResult != null)
                storeResult.Value = 0;
        }
    }
}