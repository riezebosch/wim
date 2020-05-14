namespace AzureDevOpsRest.Data
{
    public class Multiple<T>
    {
        public T[] Value { get; set; }
        public int Count { get; set; }
    }
}