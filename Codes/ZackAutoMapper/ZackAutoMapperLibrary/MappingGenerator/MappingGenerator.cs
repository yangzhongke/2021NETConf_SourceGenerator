using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Text;
namespace MyGenerator;
[Generator]
public class MappingGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxReceiver = context.SyntaxReceiver as SyntaxReceiverCreatorClass;
        if(syntaxReceiver==null)
        {
            return;
        }
        foreach (var mapperClassDec in syntaxReceiver.MappingPlans)
        {
            var typedSymbolMapperClass = FindTypeSymbol(context.Compilation, mapperClassDec);

            //namespace of mapper class
            string nsOfMapperClass = typedSymbolMapperClass.ContainingNamespace.Name;
            //classname of mapper class
            string clzNameOfMapperClass = typedSymbolMapperClass.Name;

            //IMapper<TestClass1, TestClass2> 
            var typedSymbolOfIMapper = typedSymbolMapperClass.AllInterfaces.Single();
            var genericTypesOfIMapper=typedSymbolOfIMapper.TypeArguments;
            Debug.Assert(genericTypesOfIMapper.Count() == 2);
            
            ITypeSymbol typedSymbolSource = genericTypesOfIMapper.ElementAt(0);
            ITypeSymbol typedSymbolDest = genericTypesOfIMapper.ElementAt(1);

            var sb = new StringBuilder();
            sb.AppendLine($"namespace {nsOfMapperClass};");
            sb.Append($"partial class {clzNameOfMapperClass}")
                .AppendLine("{");

            sb.Append($"public {typedSymbolDest} Map({typedSymbolSource} src)").AppendLine("{");  

            sb.AppendLine(BuildMapMethodBody(typedSymbolSource,typedSymbolDest));

            sb.AppendLine(" }");
            sb.AppendLine("}");
            context.AddSource($"{clzNameOfMapperClass}.generated.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }

    private string BuildMapMethodBody(ITypeSymbol typedSymbolSource, ITypeSymbol typedSymbolDest)
    {
        var srcProps = typedSymbolSource.GetMembers().Where(m => m.Kind == SymbolKind.Property);
        var destProps = typedSymbolDest.GetMembers().Where(m => m.Kind == SymbolKind.Property);
        //get the shared properties by Source type and dest type.
        //We cannot use srcProps.Intersect(destProps,...); see https://github.com/dotnet/roslyn-analyzers/issues/3427
        var sharedPropNames = srcProps.Select(p=>p.Name).Intersect(destProps.Select(p => p.Name));
        var sharedProps = srcProps.Where(p=>sharedPropNames.Contains(p.Name));

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{typedSymbolDest} dest = new();");
        foreach(var prop in sharedProps)
        {
            sb.AppendLine($"dest.{prop.Name}=src.{prop.Name};");
        }
        sb.AppendLine("return dest;");
        return sb.ToString();
    }

    private static INamedTypeSymbol? FindTypeSymbol(Compilation compilation, BaseTypeDeclarationSyntax node)
    {
        foreach(var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            if(semanticModel==null)
            {
                continue;
            }            
            return semanticModel.GetDeclaredSymbol(node);
        }
        return null;
    }

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                //Debugger.Launch();
            }
#endif
        Debug.WriteLine("Initalize code generator");
        // No initialization required for this one
        context.RegisterForSyntaxNotifications(() => { return new SyntaxReceiverCreatorClass(); });
    }

}

public class SyntaxReceiverCreatorClass : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> MappingPlans = new List<ClassDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        //pick up all the paritial classes which implement IMapper.
        var classDeclarationNode = syntaxNode as ClassDeclarationSyntax;
        if (classDeclarationNode == null|| classDeclarationNode.BaseList==null)
        {
            return;
        }
        if(!classDeclarationNode.Modifiers.Select(m=>m.ValueText).Contains("partial"))
        {
            return;
        }
        BaseTypeSyntax baseType = classDeclarationNode.BaseList.Types.FirstOrDefault();
        if(baseType == null)
        {
            return;
        }
        GenericNameSyntax genericName = baseType.DescendantNodes().OfType<GenericNameSyntax>().FirstOrDefault();
        if (genericName == null)
        {
            return;
        }
        if(genericName.Identifier.ValueText!="IMapper")
        {
            return;
        }
        var genericTypes = genericName.DescendantNodes().OfType<IdentifierNameSyntax>();
        if(genericTypes.Count()!=2)
        {
            return;
        }
        MappingPlans.Add(classDeclarationNode);
    }
}