using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CQRSToolkit
{
    [PublicAPI]
    public interface ICommand<in TCommand>
    {
        Task Execute(TCommand command);
    }

    [PublicAPI]
    public interface ICommand<in TCommand, TResponse>
    {
        Task<TResponse> Execute(TCommand command);
    }
}