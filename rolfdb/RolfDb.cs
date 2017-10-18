/*
* RolfDb 
* Copyright (C) 2015 Erik Johansson
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace opengd.org
{
	public class RolfDb
	{
		// List to contain all database names.
		List<string> databases;

		// List to contain two item Tuples for database name and table name. 
		List<Tuple<string, string>> tables;

		// List to contain four item Tuples for database name, table name, coulmn name and column data.
		List<Tuple<string, string, string, List<object>>> columns;

		public RolfDb ()
		{
			databases = new List<string>();
			tables = new List<Tuple<string, string>>();
			columns = new List<Tuple<string, string, string, List<object>>>();
		}

		public bool NewDatabase(string name)
		{
			if(DatabaseExist(name)) 
				return false;

			databases.Add(name);

			return true;
		}

		public bool NewTable (string database, string name)
		{
			if(!DatabaseExist(database))
				return false;

			if(TableExist(database, name))
			   return false;

			tables.Add(new Tuple<string, string>(database, name));

			return true;
		}

		public bool NewTable<T>(string database)
		{
			return NewTableFromTypeProperties(database, typeof(T));
		}

		public bool NewTableFromTypeProperties (string database, Type table)
		{
			NewTable(database, table.Name);

			foreach (var p in table.GetProperties ())
				Console.WriteLine(NewColumn (database, table.Name, p.Name));

			return true;
		}

		public bool NewColumn (string database, string table, string name)
		{
			if(!DatabaseExist(database) || !TableExist(database, table))
			   return false;

			if(ColumnExist(database, table, name))
				return false;

			columns.Add(new Tuple<string, string, string, List<object>>(database, table, name, new List<object>()));

			return true;
		}

		public IEnumerable<Tuple<string, List<object>>> GetTableByColumn(string database, string table)
		{
			if(TableExist(database, table))
				foreach(var column in columns.Where(o => o.Item1.Equals(database) && o.Item2.Equals(table)))
					yield return new Tuple<string, List<object>>(column.Item3, column.Item4);
		}

		public IEnumerable<List<Tuple<string, object>>> GetTableByRow (string database, string table)
		{
			if (TableExist (database, table)) {

				var tableColumns = columns.Where (o => o.Item1.Equals (database) && o.Item2.Equals (table));

				for(var i = 0; i < tableColumns.ElementAt(0).Item4.Count; i++) {
			
					var rowList = new List<Tuple<string, object>>();

					foreach(var column in tableColumns)
						rowList.Add(new Tuple<string, object>(column.Item3, column.Item4[i]));

					yield return rowList;
				}
			}
		}

		public IEnumerable<T> GetTable<T> (string database) where T: new()
		{
			if (DatabaseExist (database) && TableExist (database, typeof(T).Name)) {
				foreach(var rows in GetTableByRow(database, typeof(T).Name)) {

					T t = new T();

					foreach(var p in typeof(T).GetProperties()) {
						var first = rows.FirstOrDefault(o => o.Item1.Equals(p.Name));
						if(first != null)
							p.SetValue(t, first.Item2, null);
						else yield break;
					}

					yield return t;
				}
			}
		}

		public IEnumerable<object> GetColumn (string database, string table, string column)
		{
			if(!ColumnExist(database, table, column))
				return Enumerable.Empty<object>();

			return columns.FirstOrDefault(o => o.Item1.Equals(database) && o.Item2.Equals(table) && o.Item3.Equals(column)).Item4 as IEnumerable<object>;
		}

		public bool DatabaseExist (string database)
		{
			return databases.Contains(database);
		}

		public bool TableExist (string database, string table)
		{
			return (DatabaseExist(database) && 
			        tables.FirstOrDefault(o => o.Item1.Equals(database) && o.Item2.Equals(table)) != null)
				? true
				: false;
		}

		public bool ColumnExist (string database, string table, string column)
		{
			return (TableExist(database, table) && 
			   		columns.Where(o => o.Item1.Equals(database) && o.Item2.Equals(table) && o.Item3.Equals(column)).Count() > 0) 
				? true
				: false;
		}

		public bool DeleteDatabase (string database)
		{
			if(!DatabaseExist(database))
				return false;

			columns.RemoveAll(o => o.Item1.Equals(database));

			tables.RemoveAll(o => o.Item1.Equals(database));

			return databases.Remove(database);
		}

		public bool DeleteTable (string database, string table)
		{
			if(!TableExist(database, table))
				return false;

			columns.RemoveAll(o => o.Item1.Equals(database) && o.Item2.Equals(table));

			return (tables.RemoveAll(o => o.Item1.Equals(database) && o.Item2.Equals(table)) > 0) 
				? true 
				: false;
		}

		public bool DeleteColumn (string database, string table, string column)
		{
			if(!ColumnExist(database, table, column))
				return false;

			return (columns.RemoveAll(o => o.Item1.Equals(database) && o.Item2.Equals(table) && o.Item3.Equals(column)) > 0) 
				? true 
				: false;
		}

		public bool InsertRow (string database, string table, List<object> data)
		{
			if (!TableExist (database, table))
				return false;

			var tableColumns = columns.Where (o => o.Item1.Equals (database) && o.Item2.Equals (table));

			if (tableColumns.Count() != data.Count())
				return false;

			for (var i = 0; i < data.Count(); i++)
				columns[columns.IndexOf(tableColumns.ElementAt(i))].Item4.Add(data[i]);

			return true;
		}

		public int Insert (string database, object row)
		{
			//if (!TableExist (database, row.GetType ().Name))
			//	return 0;

			//var objectProperties = row.GetType ().GetProperties ();

			//var tableColumns = columns.Where (o => o.Item1.Equals (database) && o.Item2.Equals (row.GetType ().Name));

			//if (tableColumns.Count () != objectProperties.Length)
			//	return false;

			//for (var i = 0; i < objectProperties.Length; i++)
			//	columns [columns.IndexOf (tableColumns.ElementAt (i))].Item4.Add (objectProperties [i].GetValue (row, null));
			var c = 0;

			foreach (var p in row.GetType().GetProperties()) {
				var i = columns.FindIndex(o => o.Item1.Equals(database) && o.Item2.Equals(row.GetType().Name) && o.Item3.Equals(p.Name));
				if(i >= 0) {
					columns[i].Item4.Add(p.GetValue(row, null));
					c++;
				}
			}

			return (c > 0) ? 1 : 0;
		}

		public bool UpdateRow (string database, string table, int index, List<object> row)
		{
			if (!TableExist (database, table))
				return false;

			var tableColumns = columns.Where (o => o.Item1.Equals (database) && o.Item2.Equals (table));

			if (tableColumns.Count() != row.Count())
				return false;

			for (var i = 0; i < row.Count; i++)
				if(columns[columns.IndexOf(tableColumns.ElementAt(i))].Item4.Count > index) 
					columns[columns.IndexOf(tableColumns.ElementAt(i))].Item4[index] = row[i];

			return true;
		}

		public int UpdateRowFromObject (string database, int index, object row)
		{
			var c = 0;

			foreach (var p in row.GetType().GetProperties()) {
				var i = columns.FindIndex(o => o.Item1.Equals(database) && o.Item2.Equals(row.GetType().Name) && o.Item3.Equals(p.Name));
				if(i >= 0)
					if(columns[i].Item4.Count > index) {
						columns[i].Item4[index] = p.GetValue(row, null);
						c++;
					}
			}

			return (c > 0) ? 1 : 0;
		}

		public bool DeleteRow (string database, string table, int index)
		{
			if (!TableExist (database, table))
				return false;

			foreach (var column in columns.Where(o => o.Item1.Equals(database) && o.Item2.Equals(table)))
				columns[columns.IndexOf(column)].Item4.RemoveAt(index);

			return true;
		}

		public bool StoreDatabaseAsPackage (string database, string path)
		{
			return true;
		}

		public IEnumerable<string> GetTableNames (string database)
		{
			if(DatabaseExist(database))
				foreach(var table in tables.Where(o => o.Item1.Equals(database)))
					yield return table.Item2;
		}

		public IEnumerable<string> GetColumnNames (string database, string table)
		{
			if(TableExist(database, table))
				foreach(var column in columns.Where(o => o.Item1.Equals(database) && o.Item2.Equals(table)))
					yield return column.Item3;
		}

		public bool StoreDatabaseAtPath (string database, string path)
		{
			if(!DatabaseExist(database))
				return false;

			var end = (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                                        ? "/"
                                        : "\\";

			Directory.CreateDirectory (path + end + database);

			foreach (var column in columns.Where(o => o.Item1.Equals(database))) {
				Directory.CreateDirectory(path + end + column.Item1 + end + column.Item2);

				using (var file = File.OpenWrite(path + end + column.Item1 + end + column.Item2 + ".order")){
		            Byte[] columnName = 
						new UTF8Encoding(true).GetBytes(column.Item3 + "\n");
					file.Write(columnName, (int)file.Length, (int)columnName.Length);
				}

				using (var file = File.OpenWrite(path + end + column.Item1 + end + column.Item2 + end + column.Item3)) {
					foreach(var dataObject in column.Item4) {
						Byte[] data = 
							new UTF8Encoding(true).GetBytes(dataObject + "\n");

						file.Write(data, (int)file.Length, (int)data.Length);
					}
				}
			}

			return true;
		}

		public bool LoadPackage (string package, string database)
		{
			return true;
		}

		public Stream GetStream ()
		{
			return null;
		}

		public Stream GetStream (string database)
		{
			return null;
		}

		public Stream LoadStream (Stream stream)
		{
			return null;
		}

		public Stream LoadStream (Stream stream, string database)
		{
			return null;
		}

	}
}

