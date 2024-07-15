using System;

namespace Fora.Extensions
{
    public static class IntExtender
    {
        public static string ToCIKRoute(this int value)
        {
            string cikValue = $"{value}".PadLeft(10, '0');
            return $"CIK{cikValue}.json";
        }
    }
}
