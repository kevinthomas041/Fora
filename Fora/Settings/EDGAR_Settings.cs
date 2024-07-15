namespace Fora.Settings
{
    public class EDGAR_Settings
    {
        public string FormFilter { get; set; }

        public long MaxIncome = 10000000000;
        
        public double MaxIncomePercent { get; set; }

        public double MinIncomePercent { get; set; }

        public int StandardFundingPercentOff { get; set; }

        public int[] YearRange { get; set; }

        public char[] ValidLetters { get; set; }
        
        public int[] CIK_Values { get; set; }
    }
}
