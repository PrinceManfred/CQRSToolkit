using System;
using System.Threading.Tasks;

namespace CQRSToolkit
{
    public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
    {
        public Task<TResponse> Handle(TQuery query);
        public Task<bool> TryHandle(TQuery query, out TResponse response);
    }
}