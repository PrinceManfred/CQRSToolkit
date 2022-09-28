using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CQRSToolkit.DependencyInjection.Generator
{
    [Generator]
    public class ServiceInjectionGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var globalOptions = context.AnalyzerConfigOptions.GlobalOptions;

            string accessModifier = GetAccessModifier(globalOptions);
            string className = GetClassName(globalOptions);
            string outNamespace = GetNamespace(globalOptions);

            CQRSToolkitSyntaxReceiver? syntaxReceiver = (CQRSToolkitSyntaxReceiver?)context.SyntaxReceiver;
            if (syntaxReceiver is null) return;

            var injections = GetInjections(syntaxReceiver.Classes, context.Compilation);
            if(injections.Length == 0) return;

            if (TryGenerateMethods(syntaxReceiver.Methods, context, injections)) return;

            string source = SourceTemplate.GenerateSource(outNamespace, accessModifier, className,
                accessModifier, "AddCQRSToolkitGenerated",
                "Microsoft.Extensions.DependencyInjection.ServiceLifetime defaultLifetime = ServiceLifetime.Transient",
                injections, false);

            context.AddSource("CQRSToolkit.DependencyInjection.Generator.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new CQRSToolkitSyntaxReceiver());
        }

        #region Private Methods
        private string GetInjections(IEnumerable<ClassDeclarationSyntax> classes, Compilation compilation)
        {
            var injections = new StringBuilder();
            foreach (var target in classes)
            {
                var model = compilation.GetSemanticModel(target.SyntaxTree);
                if (model.GetDeclaredSymbol(target) is not INamedTypeSymbol info
                    || info.IsValueType || info.IsAbstract || info.IsUnboundGenericType || info.AllInterfaces.Length == 0) continue;

                foreach (var candidate in info.AllInterfaces)
                {
                    var constructedName = candidate.ConstructedFrom.ToString();
                    if (constructedName == "CQRSToolkit.IQueryHandler<TQuery, TResponse>"
                    || constructedName == "CQRSToolkit.ICommandHandler<TCommand>"
                    || constructedName == "ICommandResponseHandler<TCommand, TResponse>")
                    {
                        injections.AppendLine($"\t\t\tservices.Add(new ServiceDescriptor(typeof({candidate}), typeof({info}), defaultLifetime));");
                    }
                }
            }

            return injections.ToString();
        }

        private bool TryGenerateMethods(IEnumerable<MethodDeclarationSyntax> methods, GeneratorExecutionContext context, string injections)
        {
            bool hadMethod = false;
            foreach (var method in methods)
            {
                var model = context.Compilation.GetSemanticModel(method.SyntaxTree);

                if (model.GetDeclaredSymbol(method) is not IMethodSymbol info) continue;
                if (!info.IsPartialDefinition || !info.IsStatic || info.IsVirtual || info.IsAbstract || info.IsAsync) continue;
                if (info.Parameters.Length != 2
                    || info.Parameters[0].ToString() != "Microsoft.Extensions.DependencyInjection.IServiceCollection"
                    || info.Parameters[0].Name != "services"
                    || info.Parameters[1].ToString() != "Microsoft.Extensions.DependencyInjection.ServiceLifetime"
                    || info.Parameters[1].Name != "defaultLifetime") continue;

                if (info.ReturnType.ToString() != "Microsoft.Extensions.DependencyInjection.IServiceCollection") continue;

                // Invalid extension method access levels
                switch (info.DeclaredAccessibility)
                {
                    case Accessibility.ProtectedAndInternal:
                    case Accessibility.Protected:
                    case Accessibility.ProtectedOrInternal:
                        continue;
                }

                foreach (var attribute in info.GetAttributes())
                {
                    if(attribute.ToString() == "CQRSToolkit.DependencyInjection.Generator.Attributes.ServiceInjectionPointAttribute")
                    {
                        hadMethod = true;
                        ProcessMethod(context, info, injections);
                        break;
                    }
                }
            }

            return hadMethod;
        }

        private void ProcessMethod(GeneratorExecutionContext context, IMethodSymbol method, string injections)
        {
            string methodModifier = method.DeclaredAccessibility switch
            {
                
                Accessibility.Private => "private",
                Accessibility.Internal => "internal",
                Accessibility.Public => "public",
                _ => ""
            };

            string classAccessModifier = method.ContainingType.DeclaredAccessibility switch
            {
                Accessibility.Internal => "internal",
                Accessibility.Public => "public",
                _ => ""
            };

            string outNamespace = method.ContainingNamespace.ToString();
            string className = method.ContainingType.Name;
            string methodName = method.Name;
            string lifetimeParam = method.Parameters[1].HasExplicitDefaultValue ? 
                "Microsoft.Extensions.DependencyInjection.ServiceLifetime defaultLifetime" :
                "Microsoft.Extensions.DependencyInjection.ServiceLifetime defaultLifetime = ServiceLifetime.Transient";
            string source = SourceTemplate.GenerateSource(outNamespace, classAccessModifier, className,
                methodModifier, methodName, lifetimeParam, injections);

            context.AddSource($"{outNamespace}.{className}.{methodName}.g.cs", source);
        }

        private string GetAccessModifier(AnalyzerConfigOptions options)
        {
            string accessModifier = "internal";

            options.TryGetValue("build_property.CQRSToolkit_DIGen_AccessModifier", out var msBuildAccessModifier);
            if (msBuildAccessModifier is not null && msBuildAccessModifier.Trim() != "")
            {
                accessModifier = msBuildAccessModifier;
            }

            options.TryGetValue("CQRSToolkit_DIGen_AccessModifier", out var editorConfigAccessModifier);
            if (editorConfigAccessModifier is not null && editorConfigAccessModifier.Trim() != "")
            {
                accessModifier = editorConfigAccessModifier;
            }

            return accessModifier;
        }

        private string GetClassName(AnalyzerConfigOptions options)
        {
            string className = "CQRSServiceExtensions";

            options.TryGetValue("build_property.CQRSToolkit_DIGen_ClassName", out var msBuildClassName);
            if (msBuildClassName is not null && msBuildClassName.Trim() != "")
            {
                className = msBuildClassName;
            }

            options.TryGetValue("CQRSToolkit_DIGen_ClassName", out var editorConfigClassName);
            if (editorConfigClassName is not null && editorConfigClassName.Trim() != "")
            {
                className = editorConfigClassName;
            }

            return className;
        }

        private string GetNamespace(AnalyzerConfigOptions options)
        {
            string outNamespace = "CQRSToolkit.DependencyInjection";

            options.TryGetValue("build_property.CQRSToolkit_DIGen_Namespace", out var msBuildNamespace);
            if (msBuildNamespace is not null && msBuildNamespace.Trim() != "")
            {
                outNamespace = msBuildNamespace;
            }

            options.TryGetValue("build_property.CQRSToolkit_DIGen_Namespace", out var editorConfigNamespace);
            if (editorConfigNamespace is not null && editorConfigNamespace.Trim() != "")
            {
                outNamespace = editorConfigNamespace;
            }

            return outNamespace;
        }
        #endregion
    }

    class CQRSToolkitSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Classes { get; private set; } = new List<ClassDeclarationSyntax>();
        public List<MethodDeclarationSyntax> Methods { get; private set; } = new List<MethodDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Business logic to decide what we're interested in goes here
            if (syntaxNode is ClassDeclarationSyntax cds)
            {
                Classes.Add(cds);
            }
            else if(syntaxNode is MethodDeclarationSyntax method && method.AttributeLists.Count > 0)
            {
                Methods.Add(method);
            }
        }
    }
}