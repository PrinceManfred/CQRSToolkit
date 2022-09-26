//using System;
//using BasicAPI.Models;
//using CQRSToolkit;

//namespace BasicAPI.Features.Queries
//{
//    public class GetAllDudesQueryHandler : IQueryHandler<GetAllDudesQuery, Dude>
//    {

//        public Task<Dude> Handle(GetAllDudesQuery query)
//        {
//            return Task.FromResult<Dude>(new Dude { FirstName = "The", LastName = "Dude" });
//        }

//        public Task<bool> TryHandle(GetAllDudesQuery query, out Dude response)
//        {
//            response = new Dude { FirstName = "The", LastName = "Dude" };
//            return Task.FromResult<bool>(true);
//        }
//    }
//}

