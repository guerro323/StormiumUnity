namespace EudiFramework
{
    public interface IModulableComponentExecutable<TInput, TOutput>
    {
        /// <summary>
        /// Execute the component
        /// </summary>
        TOutput ExecuteAll(TInput input);
    }
}