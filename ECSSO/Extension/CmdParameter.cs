using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ECSSO.Extension
{
    public static class CmdParameter
    {
        private static DataTable CreateIdList<T>(IEnumerable<T> ids)
        {
            var table = new DataTable();
            table.Columns.Add("id", typeof(T));

            foreach (var id in ids)
            {
                table.Rows.Add(id);
            }

            return table;
        }
        private static DataTable CreateObjectList<T>(IEnumerable<T> list)
        {
            var table = new DataTable();
            foreach (PropertyInfo info in typeof(T).GetProperties()) {
                table.Columns.Add(info.Name, info.PropertyType);
            }

            foreach (var item in list)
            {
                int i = 0;
                var row = table.NewRow();
                foreach (PropertyInfo info in item.GetType().GetProperties())
                {
                    row[i] = info.GetValue(item);
                    i++;
                }
                table.Rows.Add(row);
            }

            return table;
        }

        public static SqlCommand AddParameter<T>(this SqlCommand command, string name, IEnumerable<T> ids)
        {
            var parameter = command.CreateParameter();

            parameter.ParameterName = name;
            parameter.TypeName = typeof(T).Name.ToLowerInvariant() + "_id_list";
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.Direction = ParameterDirection.Input;

            parameter.Value = CreateIdList(ids);

            command.Parameters.Add(parameter);
            return command;
        }
        public static SqlCommand AddTableParameter<T>(this SqlCommand command, string name, IEnumerable<T> ids)
        {
            var parameter = command.CreateParameter();

            parameter.ParameterName = name;
            parameter.TypeName = typeof(T).Name.ToLowerInvariant();
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.Direction = ParameterDirection.Input;

            parameter.Value = CreateObjectList(ids);

            command.Parameters.Add(parameter);
            return command;
        }
    }
}