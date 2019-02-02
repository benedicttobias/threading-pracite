using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace threading
{

	class Program
	{
		public static readonly List<string> Users = new List<string>()
		{
			"Ben",
			"Winda",
			"Budi",
			"Anna"
		};

		public static readonly List<string> Items = new List<string>()
		{
			"Bubble Tea",
			"Pork Cutlet",
			"Banana Split",
			"Chicken Taco",
			"Fried Chicken",
			"Fried Rice"
		};

		public static ConcurrentDictionary<string, int> Stocks = new ConcurrentDictionary<string, int>();

		public static int _totalQuantityBought;
		public static int _totalQuantitySold;

		static void Main(string[] args)
		{
			int workDayMillisecond = 5000;

			Console.WriteLine($"Main thread# :{Environment.CurrentManagedThreadId}");
			var first = Task.Run(() => WorkDay(Users[0], workDayMillisecond));
			var second = Task.Run(() => WorkDay(Users[1], workDayMillisecond));
			var third = Task.Run(() => WorkDay(Users[2], workDayMillisecond));
			var fourth = Task.Run(() => WorkDay(Users[3], workDayMillisecond));

			Task.WaitAll(first, second, third, fourth);

			foreach (var stock in Stocks)
			{
				Console.WriteLine($"Item: {stock.Key} \t Quantity: {stock.Value}");
			}

			
			int totalStock = Stocks.Values.Sum();
			Console.WriteLine($"Total bought: {_totalQuantityBought}");
			Console.WriteLine($"Total sold: {_totalQuantitySold}");
			Console.WriteLine($"Total stocks: {Stocks.Values.Sum()}");
			var error = totalStock + _totalQuantityBought - _totalQuantitySold;
			if (error == 0)
				Console.WriteLine("Stock levels match!");
			else
				Console.WriteLine("Error in stock level: " + error);

			Console.ReadLine();
		}

		private static void WorkDay(string user, int workDayMillisecond)
		{
			var random = new Random(DateTime.Now.Millisecond);
			var endDay = DateTime.Now.AddMilliseconds(workDayMillisecond);

			Console.WriteLine($"Executing thread: #{Thread.CurrentThread.ManagedThreadId} | Worker: {user}");

			// ReSharper disable once LoopVariableIsNeverChangedInsideLoop
			while (endDay > DateTime.Now)
			{
				var item = Items[random.Next(0, Items.Count - 1)];
				var quantity = random.Next(9) + 1;
				// Random number between 0 and 6
				var operation = random.Next(0, 7);

				if (operation == 1)
				{
					//Console.WriteLine($"Buy operation...");
					BuyOperation(item, quantity);
				}
				else
				{
					//Console.WriteLine($"Sell operation...");
					SellOperation(item, quantity);
				}
			}
		}

		private static void BuyOperation(string item, int quantity)
		{
			var totalStock = Stocks.AddOrUpdate(item, -1, (key, value) => value - quantity);

			if (totalStock >= 0)
			{
				// Item sold
				Interlocked.Add(ref _totalQuantityBought, quantity);
			}
			else
			{
				// Revert quantity
				Stocks.AddOrUpdate(item, 0, ((s, i) => i + quantity));
			}
		}

		private static void SellOperation(string item, int quantity)
		{
			Stocks.AddOrUpdate(item, quantity, (key, value) => value + quantity);
			Interlocked.Add(ref _totalQuantitySold, quantity);
		}
	}
}