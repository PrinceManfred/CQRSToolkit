using System;
using System.Threading.Tasks;

namespace CQRSToolkit
{
    public interface ICommandResponseHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
    {
        public Task<TResponse> Handle(TCommand command);
        public Task<bool> TryHandle(TCommand command, out TResponse response);
    }
}