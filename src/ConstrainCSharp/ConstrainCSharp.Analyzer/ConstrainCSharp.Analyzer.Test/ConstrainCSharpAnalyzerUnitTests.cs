using System.IO;
using ConstrainCSharp.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using VerifyCS =
    ConstrainCSharp.Analyzer.Test.CSharpCodeFixVerifier<
        ConstrainCSharp.Analyzer.ConstrainCSharpAnalyzerAnalyzer,
        ConstrainCSharp.Analyzer.ConstrainCSharpAnalyzerCodeFixProvider>;

namespace ConstrainCSharp.Analyzer.Test {
    [TestClass]
    public class ConstrainCSharpAnalyzerUnitTest {
        /// <summary>
        /// Verifying that there is only one assignment statement where a private field is declared does not trigger diagnostics
        /// 验证只存在位于声明私有字段处的一条赋值语句时不会触发诊断
        /// </summary>
        [TestMethod]
        public async Task
            OnlyOneAssignmentStatementInPrivateField_NoDiagnostic() {
            var test = @"
    using ConstrainCSharp.Attributes;

    namespace TestConstrainCSharp {

        public class Boy {
            [OnlyOneAssignmentStatement] 
            private int _age = 11;

            public int GetAge() {
               return _age;
            }
        }
    }";
            await new VerifyCS.Test {
                ReferenceAssemblies =
                    new ReferenceAssemblies("net8.0",
                        new PackageIdentity("Microsoft.NETCore.App.Ref",
                            "8.0.0"), Path.Combine("ref", "net8.0")),
                TestState = {
                    Sources = { test },
                    AdditionalReferences = {
                        typeof(OnlyOneAssignmentStatementAttribute).Assembly
                            .Location
                    }
                }
            }.RunAsync();
        }

        /// <summary>
        /// Verifying that there is only one assignment statement at the local scope does not trigger diagnostics
        /// 验证只存在位于局部作用域处的一条赋值语句时不会触发诊断
        /// </summary>
        [TestMethod]
        public async Task
            OnlyOneAssignmentStatementInLocalScope_NoDiagnostic() {
            var test = @"
    using ConstrainCSharp.Attributes;

    namespace TestConstrainCSharp {

        public class Boy {
            [OnlyOneAssignmentStatement] private int _age;

            public void SetAge(int age) {
                _age = age;
            }
        }
    }";
            await new VerifyCS.Test
            {
                ReferenceAssemblies =
                    new ReferenceAssemblies("net8.0",
                        new PackageIdentity("Microsoft.NETCore.App.Ref",
                            "8.0.0"), Path.Combine("ref", "net8.0")),
                TestState = {
                    Sources = { test },
                    AdditionalReferences = {
                        typeof(OnlyOneAssignmentStatementAttribute).Assembly
                            .Location
                    }
                }
            }.RunAsync();
        }

        /// <summary>
        /// Verify that diagnostics are triggered when there are multiple assignment statements
        /// 验证当存在多条赋值语句时会触发诊断
        /// </summary>
        [TestMethod]
        public async Task MultipleAssignmentStatements_Diagnostic() {
            var test = @"
    using ConstrainCSharp.Attributes;

    namespace TestConstrainCSharp {

        public class Boy {
            [OnlyOneAssignmentStatement] 
            private int _age = 11;

            public void SetAge(int age) {
                {|#0:_age = age|};
            }
        }
    }";
            var expected = VerifyCS.Diagnostic("ConstrainCSharpAnalyzer")
                .WithLocation(0).WithArguments("_age");
            await new VerifyCS.Test
            {
                ReferenceAssemblies =
                    new ReferenceAssemblies("net8.0",
                        new PackageIdentity("Microsoft.NETCore.App.Ref",
                            "8.0.0"), Path.Combine("ref", "net8.0")),
                TestState = {
                    Sources = { test },
                    ExpectedDiagnostics = { expected },
                    AdditionalReferences = {
                        typeof(OnlyOneAssignmentStatementAttribute).Assembly
                            .Location
                    }
                }
            }.RunAsync();
        }
    }
}