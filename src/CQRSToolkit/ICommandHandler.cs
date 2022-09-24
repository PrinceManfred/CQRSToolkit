using System;
using System.Threading.Tasks;

namespace CQRSToolkit
{
    public interface ICommandHandler<in TCommand>
    {
        public Task Handle(TCommand command);
        public Task<bool> TryHandle(TCommand command);
    }
}