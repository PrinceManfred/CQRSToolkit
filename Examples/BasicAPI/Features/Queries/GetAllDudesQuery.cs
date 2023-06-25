using System;
using BasicAPI.Models;
using CQRSToolkit;

namespace BasicAPI.Features.Queries
{
    public class GetAllDudesQuery : IQuery<GetAllDudes, Dude>
    {

        public Task<Dude> Execute(GetAllDudes dude)
        {
            return Task.FromResult<Dude>(new Dude { FirstName = "The", LastName = "Dude" });
        }
    }
}

