using System.Text;

namespace Microsoft.CodeAnalysis
{
    public static class RoslynHelper
    {
        public static string ToVisualTree(this SyntaxNode syntaxNode)
        {
            StringBuilder sb = new StringBuilder();
            Print(syntaxNode, sb, 0);
            return sb.ToString();
        }

        private static void Print(SyntaxNode syntaxNode,StringBuilder builder,int depth)
        {
            Type syntaxNodeType = syntaxNode.GetType();
            string nodeTypeName = syntaxNodeType.Name;
            string indentStr = new (' ', depth);

            StringBuilder sbProps = new StringBuilder();
            foreach(var prop in syntaxNodeType.GetProperties())
            {
                if (!prop.CanRead) continue;
                string propName = prop.Name;
                var ignoredProps = new string[] { "Parent", "Arity", "Language", "RawKind", "FullSpan", "Span", "SpanStart", "IsMissing", "IsStructuredTrivia", "HasStructuredTrivia", "ContainsSkippedText", "ContainsDirectives", "ContainsDiagnostics", "HasLeadingTrivia", "HasTrailingTrivia", "ContainsAnnotations", "OpenBraceToken"};
                if (ignoredProps.Contains(propName)) continue;
                object value = prop.GetValue(syntaxNode);
                if(value==null||string.IsNullOrWhiteSpace(value.ToString()))
                {
                    continue;
                }
                sbProps.Append($"{propName}={value};");
            }

            builder.Append(indentStr).Append(nodeTypeName).AppendLine($"({sbProps})");
            foreach(var childNode in syntaxNode.ChildNodes())
            {
                Print(childNode, builder, depth + 1);
            }
        }
    }
}
