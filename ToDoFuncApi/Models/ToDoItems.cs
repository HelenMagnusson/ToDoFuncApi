using System;
using System.Collections.Generic;
using System.Text;

namespace ToDoFuncApi.Models
{
    public class TodoItems
    {
        public List<Item> Items { get; set; } = new List<Item>();
    }
}
