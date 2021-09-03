using System.Threading.Tasks;
using System.Windows.Input;

namespace AyncAwait.Commands
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync();

        bool CanExecute();
    }
}
