namespace FirstAzureFunction
{
    public class Request
    {
        public int[] Addends { get; set; }

        public int GetSumOfIntegers()
        {
            var sum = 0;
            checked
            {
                foreach (var number in Addends)
                {
                    sum += number;
                }
            }
            return sum;
        }
    }
}
