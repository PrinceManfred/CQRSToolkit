using System;
using BasicAPI.Models;
using CQRSToolkit;

namespace BasicAPI.Features.Queries
{
    public class GetDudeQuery : IQuery<Dude>
    {
        public static readonly GetDudeQuery Value = new GetDudeQuery();

        private GetDudeQuery() { }
    }
}

