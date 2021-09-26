namespace YAD.Visualization
{
    public static class Extensions
    {
        public static double Mean(this byte[] buffer, int dataCount)
        {
            double sum = 0;

            for (int i = 0; i < dataCount; i++)
            {
                sum += buffer[i];
            }

            return sum / dataCount;
        }
    }
}
