using BasicAPI.Models;
using CQRSToolkit;

namespace BasicAPI.Features.Queries
{
    public class GetAllDudesQuery: IQuery<Dude>
    {
        public string? Huh { get; set; }
    }
}
