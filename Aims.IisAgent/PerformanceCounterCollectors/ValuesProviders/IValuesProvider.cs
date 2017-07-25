namespace Aims.IISAgent.PerformanceCounterCollectors.ValuesProviders
{
	public interface IValuesProvider
	{
		InstanceNameAndValue[] GetValues();
	}
}