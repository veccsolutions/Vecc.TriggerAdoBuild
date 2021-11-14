using System.Threading.Tasks;
using Vecc.TriggerAdoBuild.Models;

namespace Vecc.TriggerAdoBuild.Services
{
    public interface ITrigger
    {
        Task QueueItemAsync(Blob blob);
    }
}
