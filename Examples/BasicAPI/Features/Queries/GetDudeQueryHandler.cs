using System;
using BasicAPI.Models;
using CQRSToolkit;

namespace BasicAPI.Features.Queries
{
    public class GetDudeQueryHandler : IQueryHandler<GetDudeQuery, Dude>
    {

        public Task<Dude> Handle(GetDudeQuery query)
        {
            return Task.FromResult<Dude>(new Dude { FirstName = "The", LastName = "Dude" });
        }

        public Task<bool> TryHandle(GetDudeQuery query, out Dude response)
        {
            response = new Dude { FirstName = "The", LastName = "Dude" };
            return Task.FromResult<bool>(true);
        }
    }
}

