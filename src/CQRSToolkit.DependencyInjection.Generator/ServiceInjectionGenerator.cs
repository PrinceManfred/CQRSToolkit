﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CQRSToolkit.DependencyInjection.Generator
{
    [Generator]
    public class ServiceInjectionGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            CQRSToolkitSyntaxReceiver? syntaxReceiver = (CQRSToolkitSyntaxReceiver?)context.SyntaxReceiver;
            if (syntaxReceiver is null) return;

            var injections = new StringBuilder();
            foreach(var target in syntaxReceiver.Classes)
            {
                var model = context.Compilation.GetSemanticModel(target.SyntaxTree);
                if (model.GetDeclaredSymbol(target) is not INamedTypeSymbol info
                    || info.IsValueType || info.IsAbstract || info.IsUnboundGenericType || info.AllInterfaces.Length == 0) continue;

                foreach(var candidate in info.AllInterfaces)
                {
                    var constructedName = candidate.ConstructedFrom.ToString();
                    if(constructedName == "CQRSToolkit.IQueryHandler<TQuery, TResponse>"
                    || constructedName == "CQRSToolkit.ICommandHandler<TCommand>"
                    || constructedName == "ICommandResponseHandler<TCommand, TResponse>")
                    {
                        injections.AppendLine($"\tservices.AddTransient(typeof({candidate}), typeof({info}));");
                    }
                }
            }
            if(injections.Length == 0) return;
            string source = $@"// <auto-generated/>
namespace CQRSToolkit.DependencyInjection
{{
    public static class CQRSServiceExtensions
    {{
        public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddCQRSToolkitGenerated(this Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {{
        {injections}
            return services;
        }}
    }}
}}";
            context.AddSource("CQRSToolkit.DependencyInjection.Generator.g.cs", source);


        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new CQRSToolkitSyntaxReceiver());
        }
    }

    class CQRSToolkitSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Classes { get; private set; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Business logic to decide what we're interested in goes here
            if (syntaxNode is ClassDeclarationSyntax cds)
            {
                Classes.Add(cds);
            }
        }
    }
}