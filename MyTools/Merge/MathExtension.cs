public static class MathExtension
{
    public static int GetValidBaseCount(this int baseCount, params int[] groupSizes)
    {
        int adjusted = baseCount;

        while (true)
        {
            int current = adjusted;
            bool allDivisible = true;

            foreach (int groupSize in groupSizes)
            {
                if (current % groupSize != 0)
                {
                    allDivisible = false;
                    break;
                }

                current /= groupSize;
            }

            // Final stage must be even
            if (allDivisible && current % 2 == 0)
                return adjusted;

            adjusted++;
        }
    }
}
