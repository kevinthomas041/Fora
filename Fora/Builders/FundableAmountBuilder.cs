using Fora.Interfaces;
using Fora.Models;
using Fora.Settings;
using System.Linq;

namespace Fora.BusinessRules
{
    public class FundableAmountBuilder : IFundableAmountBuilder
    {
        private readonly EDGAR_Settings _settings;
        private int indexer = 1;
        
        public FundableAmountBuilder(EDGAR_Settings settings)
        {
            _settings = settings;
        }

        public AmountInfo Build(EdgarCompanyInfo companyInfo)
        {
            var amountInfo = new AmountInfo(companyInfo, indexer++);

            var formData = GetValidFormData(companyInfo);

            if (formData?.Any() != true) return amountInfo;

            SetStandardFundableAmount(formData, amountInfo);
            SetSpecialFundableAmount(companyInfo, formData, amountInfo);

            return amountInfo;
        }

        private Dictionary<int, decimal?>? GetValidFormData(EdgarCompanyInfo companyInfo)
        {
            var formDictionary = new Dictionary<int, decimal?>();

            foreach (var year in _settings.YearRange)
            {
                formDictionary.Add(year, null);
            }

            var formData = companyInfo?.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd;

            if (formData?.Any() != true) return null;

            foreach (var data in formData)
            {
                bool isValidForm = data?.Form?.Contains(_settings.FormFilter) == true;

                if (string.IsNullOrWhiteSpace(data?.Frame) || data?.Frame?.Length < 6 || !isValidForm)
                {
                    continue;
                }

                string dateValue = data?.Frame?.Substring(2, 4);
                bool isValidYear = int.TryParse(dateValue, out var formYear);

                if (!isValidYear || !formDictionary.ContainsKey(formYear))
                {
                    continue;
                }

                if (formDictionary[formYear] != null)
                {
                    formDictionary[formYear] += data.Val;
                }
                else
                {
                    formDictionary[formYear] = data.Val;
                }
            }

          return formDictionary;
        }

        private void SetStandardFundableAmount(Dictionary<int, decimal?> formData, AmountInfo amountInfo)
        {
            // Company must have income data for all years between (and including) 2018 and 2022.
            // Standard Fundable Amount is $0.
            bool invalidForAnyYear = formData.Any(c => c.Value == null);

            if (invalidForAnyYear) return;

            // Company must have had positive income in both 2021 and 2022. If they did not, their Standard Fundable Amount is $0.
            if (formData[2021].Value < 0 || formData[2022].Value < 0)
            {
                return;
            }

            // Using highest income between 2018 and 2022:
            var highestIncome = Math.Max(formData[2018].Value, formData[2022].Value);

            // If income is greater than or equal to $10B, standard fundable amount is 12.33 % of income.
            // If income is less than $10B, standard fundable amount is 21.51 % of income
            double percent = highestIncome >= _settings.MaxIncome ? _settings.MaxIncomePercent : _settings.MinIncomePercent;
            var standardFundableAmount = ((double)highestIncome / 100) * percent;
            
            amountInfo.StandardFundableAmount = Math.Round(standardFundableAmount, 2);
        }

        private void SetSpecialFundableAmount(EdgarCompanyInfo companyInfo, Dictionary<int, decimal?> currencyPairs, AmountInfo amountInfo)
        {
            // prevent divide by zero
            if (amountInfo.StandardFundableAmount <= 0 || currencyPairs == null) return;

            // Initially, the Special Fundable Amount is the same as Standard Fundable Amount.
            // If the company name starts with a vowel, add 15 % to the standard funding amount. 
            if (_settings.ValidLetters.Contains(companyInfo.EntityName.ToUpper()[0]))
            {
                amountInfo.SpecialFundableAmount = (amountInfo.StandardFundableAmount / 100) * 15;
            }

            // If the company’s 2022 income was less than their 2021 income, subtract 25 % from their standard funding amount.
            if (currencyPairs[2022] < currencyPairs[2021])
            {
                var subtractValue = (amountInfo.StandardFundableAmount / 100) * _settings.StandardFundingPercentOff;
                amountInfo.SpecialFundableAmount = amountInfo.StandardFundableAmount - subtractValue;
            }

            amountInfo.SpecialFundableAmount = Math.Round(amountInfo.SpecialFundableAmount, 2);
        }
    }
}
