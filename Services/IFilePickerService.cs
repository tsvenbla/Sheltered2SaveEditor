using System.Threading.Tasks;
using Windows.Storage;

namespace Sheltered2SaveEditor.Services;

public interface IFilePickerService
{
    Task<StorageFile?> PickFileAsync();
}