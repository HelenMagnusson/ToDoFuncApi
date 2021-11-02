using System;
using System.Collections.Generic;
using System.Text;
using ToDoFuncApi.Models;

namespace ToDoFuncApi
{
    public static class Mapper
    {
        public static ItemTableEntity ToTableEntity(this Item item)
        {
            return new ItemTableEntity
            {
                Completed = item.Completed,
                Text = item.Text,
                PartitionKey = "Todo",
                RowKey = item.Id
            };
        }

        public static Item ToItem(this ItemTableEntity itemTable)
        {
            return new Item
            {
                Id = itemTable.RowKey,
                Text = itemTable.Text,
                Completed = itemTable.Completed
            };
        }
    }
}
