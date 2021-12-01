using Microsoft.CodeAnalysis;

namespace SourceGeneratorDemo1
{
	[Generator]
	public class AddAMenthodGenerator : ISourceGenerator
	{
		public void Execute(GeneratorExecutionContext context)
		{
			context.AddSource("MyClass", "public class MyClass{public static void Test(){System.Console.WriteLine(666);}}");
		}

		public void Initialize(GeneratorInitializationContext context)
		{
		}
	}
}