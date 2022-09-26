using System;
using CQRSToolkit;

namespace BasicAPI.Features.Commands
{
    public class SetDudeCommandHandler : ICommandHandler<SetDudeCommand>
    {
        public SetDudeCommandHandler()
        {
        }

        public Task Handle(SetDudeCommand command)
        {
            Console.WriteLine($"Handle {command.dude.FirstName}");
            return Task.CompletedTask;
        }

        public Task<bool> TryHandle(SetDudeCommand command)
        {
            Console.WriteLine($"TryHandle {command.dude.FirstName}");
            return Task.FromResult<bool>(true);
        }
    }
}

