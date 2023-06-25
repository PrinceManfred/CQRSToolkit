using System;
using BasicAPI.Models;
using CQRSToolkit;

namespace BasicAPI.Features.Queries
{
    public class GetDudeQuery : IQuery<GetDude, Dude>
    {

        public Task<Dude> Execute(GetDude dude)
        {
            return Task.FromResult<Dude>(new Dude { FirstName = "The", LastName = "Dude" });
        }
    }
}

