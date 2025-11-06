using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DragonbaneCharacterSheetGenerator.Services
{
    public interface ILocalDocService
    {
        Task<(bool Success, string Message)> ImportJsonAsync();
    }

    public class LocalDocService : ILocalDocService
    {
        public async Task<(bool Success, string Message)> ImportJsonAsync()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a JSON file to import"
                });

                if (result == null)
                    return (false, "No file selected.");

                if (!result.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    return (false, "Only .json files are allowed.");

                using var source = await result.OpenReadAsync();

                // Try a few likely base directories so this works in the dev environment.
                string[] candidates = new[] {
                    Directory.GetCurrentDirectory(),
                    AppContext.BaseDirectory,
                    Environment.CurrentDirectory
                };

                string destDir = null;
                foreach (var c in candidates)
                {
                    if (c == null) continue;
                    var tryDir = Path.Combine(c, "wwwroot", "doc");
                    try
                    {
                        if (!Directory.Exists(tryDir))
                        {
                            // Create it if possible
                            Directory.CreateDirectory(tryDir);
                        }
                        // If creation succeeded, prefer this
                        destDir = tryDir;
                        break;
                    }
                    catch { /* ignore and try next */ }
                }

                if (destDir == null)
                {
                    // As a fallback use AppData
                    destDir = Path.Combine(FileSystem.AppDataDirectory, "doc");
                    Directory.CreateDirectory(destDir);
                }

                var destPath = Path.Combine(destDir, result.FileName);

                // If file exists, create a unique name
                if (File.Exists(destPath))
                {
                    var baseName = Path.GetFileNameWithoutExtension(result.FileName);
                    var ext = Path.GetExtension(result.FileName);
                    var i = 1;
                    string candidate;
                    do
                    {
                        candidate = Path.Combine(destDir, $"{baseName} ({i}){ext}");
                        i++;
                    } while (File.Exists(candidate));
                    destPath = candidate;
                }

                using var dest = File.Create(destPath);
                await source.CopyToAsync(dest);

                return (true, $"Imported to: {destPath}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
