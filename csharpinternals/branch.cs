using System;

class Program
{
	static void Main()
	{
		int x = 3;
		double y = 1.99;
		bool b = true;
		string str = "Hello";
		Foo(3,4);
	}

	static void Foo(int a, int b)
	{
		if (a > b)
			Console.WriteLine(">");
		else if (a < b)
			Console.WriteLine("<");
		else
			Console.WriteLine("=");
	}
}
