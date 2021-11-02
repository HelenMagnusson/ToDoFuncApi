using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace ToDoFuncApi.Models
{
    public class ItemTableEntity : TableEntity
    {
        public string Text { get; set; }
        public bool Completed { get; set; }
    }
}
