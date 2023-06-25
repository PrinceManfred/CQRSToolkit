using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasicAPI.Features.Commands;
using BasicAPI.Features.Queries;
using BasicAPI.Models;
using CQRSToolkit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DudeController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<Dude>> Get(
            [FromServices] IQuery<GetDude, Dude> getDudeQuery)
        {
            return Ok(await getDudeQuery.Execute(GetDude.Value));
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] SetDude dude,
            [FromServices] ICommand<SetDude> setDudeCommand)
        {
            await setDudeCommand.Execute(dude);
            return Ok();
        }
    }
}
