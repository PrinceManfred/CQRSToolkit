using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CQRSToolkit
{
    [PublicAPI]
    public interface IQuery<in TQuery, TResponse>
    {
        Task<TResponse> Execute(TQuery dudeParams);
    }
}