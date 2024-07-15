using Fora.Models;

namespace Fora.Interfaces
{
    public interface IFundableAmountBuilder
    {
        AmountInfo Build(EdgarCompanyInfo companyInfo);
    }
}
