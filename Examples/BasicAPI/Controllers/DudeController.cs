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
        // GET: api/asdf
        [HttpGet]
        public async Task<ActionResult<Dude>> Get(
            [FromServices] IQueryHandler<GetDudeQuery, Dude> getDudeQueryHandler)
        {
            return Ok(await getDudeQueryHandler.Handle(GetDudeQuery.Value));
        }

        // GET: api/asdf/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/asdf
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] SetDudeCommand command,
            [FromServices] ICommandHandler<SetDudeCommand> setDudeCommandHandler)
        {
            await setDudeCommandHandler.Handle(command);
            return Ok();
        }

        // PUT: api/asdf/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/asdf/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
