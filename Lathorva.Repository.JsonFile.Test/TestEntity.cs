using System;
using System.Collections.Generic;
using System.Text;
using Lathorva.Common.Repository.Models;

namespace Lathorva.Repository.JsonFile.Test
{
    public class TestEntity : IEntityDate<string>
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<int> TestArray { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
