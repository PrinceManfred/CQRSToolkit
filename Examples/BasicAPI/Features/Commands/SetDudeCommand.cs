using CQRSToolkit;

namespace BasicAPI.Features.Commands
{
    public class SetDudeCommand : ICommand<SetDude>
    {
        public Task Execute(SetDude dude)
        {
            Console.WriteLine($"Handle {dude.Dude.FirstName}");
            return Task.CompletedTask;
        }
    }
}

