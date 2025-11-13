using Microsoft.Maui.Storage;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DragonbaneCharacterSheetGenerator.Services
{
    public interface IFavoritesService
    {
        Task ClearAllAsync();
        Task<List<string>> GetAllAsync();
        Task AddAsync(string id);
    }

    public class FavoritesService : IFavoritesService
    {
        private const string PrefKey = "favorites_list";

        public Task ClearAllAsync()
        {
            Preferences.Remove(PrefKey);
            return Task.CompletedTask;
        }

        public Task<List<string>> GetAllAsync()
        {
            var json = Preferences.Get(PrefKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json)) return Task.FromResult(new List<string>());
            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(json);
                return Task.FromResult(list ?? new List<string>());
            }
            catch
            {
                return Task.FromResult(new List<string>());
            }
        }

        public async Task AddAsync(string id)
        {
            var list = await GetAllAsync();
            if (!list.Contains(id)) list.Add(id);
            Preferences.Set(PrefKey, JsonSerializer.Serialize(list));
        }
    }
}
