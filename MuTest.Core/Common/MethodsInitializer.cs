﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Model;
using MuTest.Core.Mutants;
using MuTest.Core.Utility;
using static MuTest.Core.Common.Constants;

namespace MuTest.Core.Common
{
    public class MethodsInitializer
    {
        public bool IncludeNestedClasses { get; set; } = false;

        public async Task FindMethods(SourceClassDetail source)
        {
            if (source.Claz == null)
            {
                return;
            }

            var getClass = await source.Claz.SetFields();
            source.Claz = getClass
                .DescendantNodes<ClassDeclarationSyntax>()
                .FirstOrDefault(x => x.ClassName() == source.Claz.ClassName());
            source.MethodDetails.Clear();

            var index = 1;
            IList<MethodDeclarationSyntax> sourceMethods;
            IList<ConstructorDeclarationSyntax> constructorMethods;
            IList<PropertyDeclarationSyntax> sourceProperties;
            if (IncludeNestedClasses)
            {
                sourceMethods = source
                    .Claz
                    .Root()
                    .GetMethods()
                    .OrderBy(x => x.MethodName()).ToList();

                constructorMethods = source
                    .Claz
                    .Root()
                    .DescendantNodes<ConstructorDeclarationSyntax>()
                    .OrderBy(x => x.MethodName())
                    .ToList();

                sourceProperties = source
                    .Claz
                    .Root()
                    .DescendantNodes<PropertyDeclarationSyntax>()
                    .Where(x => MutantOrchestrator.GetDefaultMutants(x).Any())
                    .OrderBy(x => x.Identifier.ValueText)
                    .ToList();
            }
            else
            {
                var parentClassNodesCount = source.Claz?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().Count() ?? 0;
                sourceMethods = source
                    .Claz
                    .GetMethods()
                    .Where(x => x.Ancestors<ClassDeclarationSyntax>().Count == parentClassNodesCount)
                    .OrderBy(x => x.MethodName())
                    .ToList();

                constructorMethods = source
                    .Claz
                    .DescendantNodes<ConstructorDeclarationSyntax>()
                    .Where(x => x.Ancestors<ClassDeclarationSyntax>().Count == parentClassNodesCount)
                    .OrderBy(x => x.MethodName())
                    .ToList();

                sourceProperties = source
                    .Claz
                    .DescendantNodes<PropertyDeclarationSyntax>()
                    .Where(x => x.Ancestors<ClassDeclarationSyntax>().Count == parentClassNodesCount)
                    .Where(x => MutantOrchestrator.GetDefaultMutants(x).Any())
                    .OrderBy(x => x.Identifier.ValueText)
                    .ToList();
            }

            source.MethodDetails.AddRange(sourceMethods
                .Select(x =>
                    new MethodDetail
                    {
                        MethodName = $"{index++}. {x.MethodName()}{x.DescendantNodes<ParameterListSyntax>().FirstOrDefault()}",
                        Method = x,
                        IsOverrideMethod = x.IsOverride()
                    }));


            source.MethodDetails.AddRange(constructorMethods
                .Select(x =>
                    new MethodDetail
                    {
                        MethodName = $"{index++}. {x.MethodName()}{x.DescendantNodes<ParameterListSyntax>().FirstOrDefault()}",
                        Method = x,
                        IsConstructor = true
                    }));

            source.MethodDetails.AddRange(sourceProperties
                .Select(x => new MethodDetail
                {
                    MethodName = $"{index++}. Property - {x.Identifier.ValueText}",
                    Method = x,
                    IsProperty = true
                }));

            source.TestClaz.MethodDetails.Clear();
            if (source.TestClaz.BaseClass != null)
            {
                source.TestClaz.MethodDetails.AddRange(GetTestMethods(source.TestClaz.BaseClass));
            }

            foreach (var partialClass in source.TestClaz.PartialClasses)
            {
                source.TestClaz.MethodDetails.AddRange(GetTestMethods(partialClass.Claz));
            }
        }

        private static IEnumerable<MethodDetail> GetTestMethods(SyntaxNode node)
        {
            return node.GetMethods()
                .Where(x => x.AttributeLists
                    .SelectMany(att => att.Attributes)
                    .Any(y => !UnitTestsSetupAttributes
                        .Any(z => y.Name.ToString().StartsWith(z, StringComparison.InvariantCultureIgnoreCase))))
                .Select(x =>
                    new MethodDetail
                    {
                        MethodName = $"{x.MethodName()}{x.DescendantNodes<ParameterListSyntax>().FirstOrDefault()}",
                        Method = x
                    }).OrderBy(x => x.MethodName);
        }
    }
}