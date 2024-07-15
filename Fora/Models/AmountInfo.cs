using System.Text.Json.Serialization;

namespace Fora.Models
{
    public class AmountInfo 
    {
        public AmountInfo(EdgarCompanyInfo companyInfo, int index)
        {
            Name = companyInfo.EntityName;
            Index = index;
        }

        [JsonPropertyName("id")]
        public int Index { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("standardFundableAmount")]
        public double StandardFundableAmount { get; set; } = 0;

        [JsonPropertyName("specialFundableAmount")]
        public double SpecialFundableAmount { get; set; } = 0;
    }
}
