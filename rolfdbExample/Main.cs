using System;
using opengd.org;
using System.Collections.Generic;

namespace RolfDbExample
{
	class Contacts
	{
		public string Firstname
		{
			get; set;
		}

		public string Surename
		{
			get; set;
		}

		public string Email 
		{
			get; set;
		}

		public int Id {
			get;
			set;
		}
	}

	class MainClass
	{
		public static void Main (string[] args)
		{
			var rolfDb = new RolfDb ();


			var prp = typeof(RolfDb).GetProperties ();

			Console.WriteLine(rolfDb.GetType().Name);

			foreach (var p in prp) {
				Console.WriteLine(p.Name + " " + p.PropertyType);
			}

			var database = "rolf";

			Console.WriteLine ("New database: " + rolfDb.NewDatabase (database));

			Console.WriteLine ("New table: " + rolfDb.NewTable (database, "ett"));

			Console.WriteLine ("New table from type: " + rolfDb.NewTable<Contacts> (database));

			Console.WriteLine ("New column: " + rolfDb.NewColumn (database, "ett", "etta"));

			Console.WriteLine ("New column: " + rolfDb.NewColumn (database, "ett", "tva"));

			var rand = new System.Random ();

			for (var i = 0; i < 10; i++)
				Console.WriteLine ("Insert row: " + rolfDb.InsertRow (database, "ett", new List<object> () {rand.Next().ToString(), rand.Next().ToString()}));

			Console.WriteLine ("Insert row: " + rolfDb.InsertRow (database, "ett", new List<object> () {"test", "test2"}));

			Console.WriteLine ("Update row 5: " + rolfDb.UpdateRow (database, "ett", 5, new List<object> () {"tilla", "filla"}));

			/*
			foreach (var s in tassla.GetColumn(database, "ett", "etta"))
				Console.WriteLine ((string)s);

			foreach (var s in tassla.GetTableByColumn(database, "ett"))
				Console.WriteLine (s.Item1);
			*/

			foreach (var s in rolfDb.GetTableNames(database))
				Console.WriteLine (s);

			foreach (var s in rolfDb.GetColumnNames(database, typeof(Contacts).Name))
				Console.WriteLine (s);

			var c = new Contacts() {Firstname = "Kalle", Surename = "Palle", Email = "kalle.palle@mail.com", Id = 0};
			var g = new Contacts() {Firstname = "Gittan", Surename = "Kik", Email = "Gittan.Kik@mail.com", Id = 1};

			Console.WriteLine ("Insert row from object: " + rolfDb.Insert(database, c));
			Console.WriteLine ("Insert row from object: " + rolfDb.Insert(database, g));

			c.Email = "raisorblade@nasty.com";

			Console.WriteLine ("Update row from object: " + rolfDb.UpdateRowFromObject(database, 0, c));

			foreach (var row in rolfDb.GetTableByRow(database, typeof(Contacts).Name))
				foreach(var column in row)
					Console.WriteLine("Column: " + column.Item1 + " Data: " + column.Item2);

			foreach(var con in rolfDb.GetTable<Contacts>(database))
				Console.WriteLine(con.Firstname + " " + con.Surename + " " + con.Email + " " + con.Id); 

			//Console.WriteLine("Store database in path: " + tassla.StoreDatabaseAtPath(database, "/home/pi/databases"));

			Console.WriteLine (database);
		}
	}
}
