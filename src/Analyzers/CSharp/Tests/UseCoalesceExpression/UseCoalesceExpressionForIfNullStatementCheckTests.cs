﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Analyzers.UseCoalesceExpression;
using Microsoft.CodeAnalysis.CSharp.UseCoalesceExpression;
using Microsoft.CodeAnalysis.Editor.UnitTests.CodeActions;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.Analyzers.UnitTests.UseCoalesceExpression;

using VerifyCS = CSharpCodeFixVerifier<
    CSharpUseCoalesceExpressionForIfNullStatementCheckDiagnosticAnalyzer,
    CSharpUseCoalesceExpressionForIfNullStatementCheckCodeFixProvider>;

[Trait(Traits.Feature, Traits.Features.CodeActionsUseCoalesceExpression)]
public sealed class UseCoalesceExpressionForIfNullStatementCheckTests
{
    [Fact]
    public async Task TestLocalDeclaration_ThrowStatement()
    {
        await new VerifyCS.Test
        {
            TestCode = """
            class C
            {
                void M()
                {
                    var item = FindItem() as C;
                    [|if|] (item == null)
                        throw new System.InvalidOperationException();
                }

                object FindItem() => null;
            }
            """,
            FixedCode = """
            class C
            {
                void M()
                {
                    var item = FindItem() as C ?? throw new System.InvalidOperationException();
                }
            
                object FindItem() => null;
            }
            """
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_Block()
    {
        await new VerifyCS.Test
        {
            TestCode = """
            class C
            {
                void M()
                {
                    var item = FindItem() as C;
                    [|if|] (item == null)
                    {
                        throw new System.InvalidOperationException();
                    }
                }

                object FindItem() => null;
            }
            """,
            FixedCode = """
            class C
            {
                void M()
                {
                    var item = FindItem() as C ?? throw new System.InvalidOperationException();
                }
            
                object FindItem() => null;
            }
            """
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_IsPattern()
    {
        await new VerifyCS.Test
        {
            TestCode = """
            class C
            {
                void M()
                {
                    var item = FindItem() as C;
                    [|if|] (item is null)
                        throw new System.InvalidOperationException();
                }

                object FindItem() => null;
            }
            """,
            FixedCode = """
            class C
            {
                void M()
                {
                    var item = FindItem() as C ?? throw new System.InvalidOperationException();
                }
            
                object FindItem() => null;
            }
            """
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_Assignment1()
    {
        await new VerifyCS.Test
        {
            TestCode = """
            class C
            {
                void M()
                {
                    var item = FindItem() as C;
                    [|if|] (item == null)
                        item = new C();
                }

                object FindItem() => null;
            }
            """,
            FixedCode = """
            class C
            {
                void M()
                {
                    var item = FindItem() as C ?? new C();
                }
            
                object FindItem() => null;
            }
            """
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_Assignment2()
    {
        await new VerifyCS.Test
        {
            TestCode = """
            class C
            {
                void M()
                {
                    var item = FindItem() as C;
                    [|if|] (item == null)
                        item = new();
                }

                object FindItem() => null;
            }
            """,
            FixedCode = """
            class C
            {
                void M()
                {
                    var item = FindItem() as C ?? new();
                }
            
                object FindItem() => null;
            }
            """,
            LanguageVersion = LanguageVersion.CSharp9,
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_NotWithWrongItemChecked()
    {
        var text = """
            class C
            {
                void M(C item1)
                {
                    var item = FindItem() as C;
                    if (item1 == null)
                        throw new System.InvalidOperationException();
                }

                object FindItem() => null;
            }
            """;

        await new VerifyCS.Test
        {
            TestCode = text,
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_NotWithWrongCondition()
    {
        var text = """
            class C
            {
                void M()
                {
                    var item = FindItem() as C;
                    if (item != null)
                        throw new System.InvalidOperationException();
                }

                object FindItem() => null;
            }
            """;

        await new VerifyCS.Test
        {
            TestCode = text,
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_NotWithWrongPattern()
    {
        var text = """
            class C
            {
                void M()
                {
                    var item = FindItem() as C;
                    if (item is not null)
                        throw new System.InvalidOperationException();
                }

                object FindItem() => null;
            }
            """;

        await new VerifyCS.Test
        {
            TestCode = text,
            LanguageVersion = LanguageVersion.CSharp9,
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_NotWithWrongAssignment()
    {
        var text = """
            class C
            {
                void M(C item1)
                {
                    var item = FindItem() as C;
                    if (item == null)
                        item1 = new C();
                }

                object FindItem() => null;
            }
            """;

        await new VerifyCS.Test
        {
            TestCode = text,
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_NotWithElseBlock()
    {
        var text = """
            class C
            {
                void M(C item1)
                {
                    var item = FindItem() as C;
                    if (item == null)
                        item = new C();
                    else
                        item = null;
                }

                object FindItem() => null;
            }
            """;

        await new VerifyCS.Test
        {
            TestCode = text,
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_NotWithMultipleWhenTrueStatements()
    {
        var text = """
            class C
            {
                void M(C item1)
                {
                    var item = FindItem() as C;
                    if (item == null)
                    {
                        item = new C();
                        item = null;
                    }
                }

                object FindItem() => null;
            }
            """;

        await new VerifyCS.Test
        {
            TestCode = text,
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_NotWithNoWhenTrueStatements()
    {
        var text = """
            class C
            {
                void M(C item1)
                {
                    var item = FindItem() as C;
                    if (item == null)
                    {
                    }
                }

                object FindItem() => null;
            }
            """;

        await new VerifyCS.Test
        {
            TestCode = text,
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_NotWithThrowWithoutExpression()
    {
        var text = """
            class C
            {
                void M()
                {
                    try
                    {
                    }
                    catch
                    {
                        var item = FindItem() as C;
                        if (item == null)
                            throw;
                    }
                }

                object FindItem() => null;
            }
            """;

        await new VerifyCS.Test
        {
            TestCode = text,
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_NotWithLocalWithoutInitializer()
    {
        var text = """
            class C
            {
                void M()
                {
                    C item;
                    if ({|CS0165:item|} == null)
                        throw new System.InvalidOperationException();
                }

                object FindItem() => null;
            }
            """;

        await new VerifyCS.Test
        {
            TestCode = text,
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_NotWithValueTypeInitializer()
    {
        var text = """
            class C
            {
                void M()
                {
                    object item = 0;
                    if (item == null)
                        item = null;
                }

                object FindItem() => null;
            }
            """;

        await new VerifyCS.Test
        {
            TestCode = text,
        }.RunAsync();
    }

    [Fact]
    public async Task TestLocalDeclaration_NotWithReferenceToVariableInThrow()
    {
        var text = """
            class C
            {
                void M()
                {
                    var item = FindItem() as C;
                    if (item is null)
                        throw new System.InvalidOperationException(nameof(item));
                }

                object FindItem() => null;
            }
            """;

        await new VerifyCS.Test
        {
            TestCode = text,
            LanguageVersion = LanguageVersion.CSharp9,
        }.RunAsync();
    }

    [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/74460")]
    public async Task TestLocalDeclaration_CastWithParenthesizedExpression()
    {
        await new VerifyCS.Test
        {
            TestCode =
            """
            interface I
            {
            }

            class C : I
            {
                void M(object o)
                {
                    I item = o as C;
                    [|if|] (item == null)
                    {
                        item = o as D;
                    }
                }
            }

            class D : I
            {
            }
            """,
            FixedCode =
            """
            interface I
            {
            }

            class C : I
            {
                void M(object o)
                {
                    I item = (I)(o as C) ?? o as D;
                }
            }

            class D : I
            {
            }
            """
        }.RunAsync();
    }

    [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/74460")]
    public async Task TestLocalDeclaration_CastWithoutParenthesizedExpression()
    {
        await new VerifyCS.Test
        {
            TestCode =
            """
            interface I
            {
            }

            class C : I
            {
                void M(C c, D d)
                {
                    I item = c;
                    [|if|] (item == null)
                    {
                        item = d;
                    }
                }
            }

            class D : I
            {
            }
            """,
            FixedCode =
            """
            interface I
            {
            }

            class C : I
            {
                void M(C c, D d)
                {
                    I item = (I)c ?? d;
                }
            }

            class D : I
            {
            }
            """
        }.RunAsync();
    }

    [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/74460")]
    public async Task TestLocalDeclaration_NoCastWhenEqualSymbol()
    {
        await new VerifyCS.Test
        {
            TestCode =
            """
            interface I
            {
            }

            class C : I
            {
                void M(C c1, C c2)
                {
                    I item = c1;
                    [|if|] (item == null)
                    {
                        item = c2;
                    }
                }
            }
            """,
            FixedCode =
            """
            interface I
            {
            }

            class C : I
            {
                void M(C c1, C c2)
                {
                    I item = c1 ?? c2;
                }
            }
            """
        }.RunAsync();
    }

    [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/74460")]
    public async Task TestLocalDeclaration_NoCastWhenDerivedClass()
    {
        await new VerifyCS.Test
        {
            TestCode =
            """
            interface I
            {
            }

            class C : I
            {
                void M(C c, D d)
                {
                    I item = c;
                    [|if|] (item == null)
                    {
                        item = d;
                    }
                }
            }

            class D : C
            {
            }
            """,
            FixedCode =
            """
            interface I
            {
            }

            class C : I
            {
                void M(C c, D d)
                {
                    I item = c ?? d;
                }
            }

            class D : C
            {
            }
            """
        }.RunAsync();
    }

    [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/74460")]
    public async Task TestLocalDeclaration_NoCastWhenDerivedClassReversed()
    {
        await new VerifyCS.Test
        {
            TestCode =
            """
            interface I
            {
            }

            class C : D
            {
                void M(C c, D d)
                {
                    I item = c;
                    [|if|] (item == null)
                    {
                        item = d;
                    }
                }
            }

            class D : I
            {
            }
            """,
            FixedCode =
            """
            interface I
            {
            }

            class C : D
            {
                void M(C c, D d)
                {
                    I item = c ?? d;
                }
            }

            class D : I
            {
            }
            """
        }.RunAsync();
    }

    [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/70514")]
    public async Task TestNotAcrossPreprocessorRegion()
    {
        await new VerifyCS.Test
        {
            TestCode = """
                #define DEBUG

                class C
                {
                    void M()
                    {
                        var item = FindItem() as C;
                #if DEBUG
                        if (item == null)
                            throw new System.InvalidOperationException();
                #endif
                    }

                    object FindItem() => null;
                }
                """,
        }.RunAsync();
    }
}
