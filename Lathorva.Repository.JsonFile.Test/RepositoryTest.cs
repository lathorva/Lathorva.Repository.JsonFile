using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lathorva.Common.Extensions;
using Lathorva.Common.Repository.Filtering;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lathorva.Repository.JsonFile.Test
{
    [TestClass]
    public class RepositoryTest
    {
        private readonly string _jsonFileName;
        private readonly JsonFileRepository<TestEntity, FilterModel> _jsonFileRepository;
        public RepositoryTest()
        {
            _jsonFileName = Path.Combine(Directory.GetCurrentDirectory(), StringExtensions.GetRandomString(10) + ".json");
            _jsonFileRepository = new JsonFileRepository<TestEntity, FilterModel>(_jsonFileName);
        }
        [TestMethod]
        public async Task CrudTest()
        {
            var random = new Random();
            var crudResult = await _jsonFileRepository.CreateAsync(new TestEntity
            {
                Name = StringExtensions.GetRandomString(10),
                TestArray = new List<int> {random.Next(), random.Next()}
            });
            var crudResult2 = await _jsonFileRepository.CreateAsync(new TestEntity
            {
                Name = StringExtensions.GetRandomString(10),
                TestArray = new List<int> { random.Next(), random.Next() }
            });

            Assert.IsTrue(crudResult.Ok);
            Assert.IsTrue(crudResult.Model.CreatedDate == crudResult.Model.UpdatedDate);
            Assert.IsTrue(crudResult.Model.CreatedDate.Date == DateTimeOffset.Now.Date);

            crudResult.Model.Name = "123";
            crudResult.Model.TestArray.RemoveAt(0);

            var result2 = await _jsonFileRepository.UpdateAsync(crudResult.Model.Id, crudResult.Model);

            Assert.IsTrue(result2.Model.UpdatedDate > result2.Model.CreatedDate);
            Assert.IsTrue(result2.Model.TestArray.Count == 1);

            var totalCount = await _jsonFileRepository.CountAsync(new FilterModel());

            Assert.IsTrue(totalCount == 2);

            var result = await _jsonFileRepository.DeleteAsync(crudResult2.Model.Id);

            if (result.Ok)
            {
                Assert.IsTrue(await _jsonFileRepository.CountAsync(new FilterModel()) == 1);
            }
        }

        [ClassCleanup]
        public void Cleanup()
        {
            File.Delete(_jsonFileName);
        }
    }
}
