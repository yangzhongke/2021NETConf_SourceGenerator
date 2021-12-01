using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;

namespace 自动为部分类增加ToString方法;
[Generator]
public class AddAMenthodGenerator : ISourceGenerator
{
	public void Execute(GeneratorExecutionContext context)
	{
		var namespaceNodes = context.Compilation.SyntaxTrees.SelectMany(t => t.GetRoot().DescendantNodesAndSelf().OfType<NamespaceDeclarationSyntax>());
		foreach (var namespaceNode in namespaceNodes)
		{
			string nsValue = ((IdentifierNameSyntax)namespaceNode.Name).Identifier.Text;
			var classDef = namespaceNode.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
			if (classDef == null)
				continue;
			if (!classDef.Modifiers.Select(m => m.Text).Any(m => m == "partial" && m != "static"))
			{
				continue;
			}
			//如果定义了ToString方法，则跳过
			if (classDef.DescendantNodes().OfType<MethodDeclarationSyntax>().Any(m => m.Identifier.Text == "ToString"))
				continue;
			string className = classDef.Identifier.Text;
			var propDeclNodes = classDef.DescendantNodes().OfType<PropertyDeclarationSyntax>();
			StringBuilder sb = new StringBuilder();
			sb.Append("namespace ").Append(nsValue).AppendLine("{");
			sb.Append("public partial class ").Append(className).AppendLine("{");
			sb.AppendLine("public override string ToString(){");
			sb.Append("return \"\"");
			foreach (var propNode in propDeclNodes)
			{
				string propName = propNode.Identifier.Text;
				sb.Append("+").Append("\" ").Append(propName).Append("=\"+").Append(propName);
			}
			sb.AppendLine(";");
			sb.AppendLine("}");
			sb.AppendLine("}");
			sb.AppendLine("}");
			context.AddSource(className + ".generated.cs", sb.ToString());
		}
	}

	public void Initialize(GeneratorInitializationContext context)
	{
	}
}
