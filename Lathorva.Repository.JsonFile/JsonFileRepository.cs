using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lathorva.Common.Repository;
using Lathorva.Common.Repository.Filtering;
using Lathorva.Common.Repository.Models;
using Newtonsoft.Json;

namespace Lathorva.Repository.JsonFile
{
    public class JsonFileRepository<TModel, TFilter> : IRepository<string, TModel, TFilter> 
        where TModel : class, IEntity<string> 
        where TFilter : IFilterModel
    {
        private readonly string _jsonFilePath;
        private List<TModel> _data;

        public JsonFileRepository(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath;
            InitilizeData();
        }

        private void InitilizeData()
        {
            if (File.Exists(_jsonFilePath))
            {

                var json = System.IO.File.ReadAllText(_jsonFilePath);
                _data = JsonConvert.DeserializeObject<List<TModel>>(json);
            }
            else
            {
                File.Create(_jsonFilePath);
                _data = new List<TModel>();
            }
        }

        private async Task SaveDataToJsonFileAsync()
        {
            var json = JsonConvert.SerializeObject(_data, Formatting.Indented);
            await File.WriteAllTextAsync(_jsonFilePath, json);
        }

        private void SetDatesIfExists(TModel model, bool updateOnly)
        {
            if (model is IEntityDate<string> s)
            {
                var now = DateTimeOffset.Now;

                if(!updateOnly) s.CreatedDate = now;
                s.UpdatedDate = now;
            }
        }


        public async Task<ICrudResult<string, TModel>> CreateAsync(TModel model)
        {
            model.Id = Guid.NewGuid().ToString();

            SetDatesIfExists(model, false);

            _data.Add(model);
            await SaveDataToJsonFileAsync();

            return CrudResultString<TModel>.CreateOk(model);
        }

        public async Task<ICrudResult<string, TModel>> UpdateAsync(string id, TModel model)
        {
            var savedModelWithIndex = _data.Select((m, index) => new {m, index}).SingleOrDefault(e => e.m.Id == id);

            if (savedModelWithIndex == null) return CrudResultString<TModel>.CreateNotFound();
            model.Id = id;
            SetDatesIfExists(model, true);

            _data[savedModelWithIndex.index] = model;

            await SaveDataToJsonFileAsync();

            return CrudResultString<TModel>.CreateOk(model);
        }

        public async Task<ICrudResult<string, TModel>> DeleteAsync(string id)
        {
            var savedModel = _data.SingleOrDefault(e => e.Id == id);

            if (savedModel == null) return CrudResultString<TModel>.CreateNotFound();

            _data.Remove(savedModel);

            await SaveDataToJsonFileAsync();

            return CrudResultString<TModel>.CreateOk();
        }

        public void Dispose()
        {
            _data = null;
        }

        public async Task<TModel> GetByIdOrDefaultAsync(string id)
        {
            return await Task.Run(() =>
                {
                    var savedModel = _data.SingleOrDefault(e => e.Id == id);

                    return savedModel;
                }
            );
        }

        public async Task<PagedResult<string, TModel>> GetAllAsync(TFilter filterModel)
        {
            return await Task.Run(() =>
                {
                    var pagedResult = _data.Skip(filterModel.Offset).Take(filterModel.Limit);
                    return new PagedResult<string, TModel>(pagedResult, _data.Count, filterModel);
                }
            );
        }

        public async Task<int> CountAsync(TFilter filterModel)
        {
            return await Task.Run(() => _data.Count);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await GetByIdOrDefaultAsync(id) != null;
        }

        public virtual bool AlwaysDisablePaging => false;
    }

}
