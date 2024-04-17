using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace ConstrainCSharp.Analyzer {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConstrainCSharpAnalyzerAnalyzer : DiagnosticAnalyzer {
        public const string DiagnosticId = "ConstrainCSharpAnalyzer";

        /// <summary>
        /// 使私有字段只存在一个赋值语句
        /// </summary>
        private const string OnlyOneAssignmentStatementAttribute =
            "OnlyOneAssignmentStatement";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title =
            new LocalizableResourceString(nameof(Resources.AnalyzerTitle),
                Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(
                nameof(Resources.AnalyzerMessageFormat),
                Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.AnalyzerDescription),
                Resources.ResourceManager, typeof(Resources));

        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
                Category, DiagnosticSeverity.Error, isEnabledByDefault: true,
                description: Description);

        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics {
            get { return ImmutableArray.Create(Rule); }
        }

        public override void Initialize(AnalysisContext context) {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags
                .None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeNode,
                SyntaxKind.SimpleAssignmentExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context) {
            // 取局部变量名
            var assignmentExpressionSyntaxNode =
                (AssignmentExpressionSyntax)context.Node;
            var localVariableName =
                (assignmentExpressionSyntaxNode.Left as IdentifierNameSyntax)
                ?.Identifier.ValueText;

            // 检查局部变量名是否是标记上ReadonlyAfterAssignment特性的私有字段名,是则取出字段声明语法节点,如果不是则返回
            var syntaxTree = assignmentExpressionSyntaxNode.SyntaxTree;
            var rootNode = syntaxTree.GetCompilationUnitRoot();

            var targetFieldDeclarationSyntaxNode = rootNode.DescendantNodes()
                .OfType<AttributeSyntax>()
                .Where(a =>
                    (a.Name as IdentifierNameSyntax)?.Identifier.ValueText ==
                    OnlyOneAssignmentStatementAttribute)
                .Select(a => (FieldDeclarationSyntax)a.Parent.Parent)
                .SingleOrDefault(f =>
                    f.Declaration.Variables.Any(v =>
                        v.Identifier.ValueText == localVariableName));
            if (targetFieldDeclarationSyntaxNode is null ||
                !targetFieldDeclarationSyntaxNode.Modifiers.Any(SyntaxKind
                    .PrivateKeyword))
                return;

            // 检查标记上ReadonlyAfterAssignment特性的同名字段是否被赋值
            var hasAssignedInField = targetFieldDeclarationSyntaxNode
                .Declaration.Variables
                .Single(v => v.Identifier.ValueText == localVariableName)
                .DescendantNodes()
                .Any(n => n.IsKind(SyntaxKind.EqualsValueClause));

            // 计算目标局部变量赋值次数
            var localVariableAssignedCount = rootNode.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Where(a => a.IsKind(SyntaxKind.SimpleAssignmentExpression))
                .Count(a =>
                    ((IdentifierNameSyntax)a.Left).Identifier.ValueText ==
                    localVariableName);

            // 计算总赋值次数
            var totalAssignedCount = localVariableAssignedCount +
                (hasAssignedInField ? 1 : 0);

            // 当总赋值次数超过一次则报告错误
            if (totalAssignedCount > 1)
                context.ReportDiagnostic(Diagnostic.Create(Rule,
                    assignmentExpressionSyntaxNode.GetLocation(), localVariableName));
        }
    }
}