namespace Aims.IISAgent.Pipes
{
	public interface IRunnable
	{
		/// <summary>
		/// Gets a value indicating whether this instance is running.
		/// </summary>
		bool IsRunning { get; }

		/// <summary>
		/// Starts this instance.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops this instance.
		/// </summary>
		void Stop();
	}
}