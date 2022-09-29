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

            CQRSToolkitSyntaxReceiver syntaxReceiver = (CQRSToolkitSyntaxReceiver)context.SyntaxReceiver;
            if (syntaxReceiver is null) return;

            var injectionStubs = GetInjections(syntaxReceiver.Classes, context.Compilation);
            if(injectionStubs.Count == 0) return;

            if (TryGenerateMethods(syntaxReceiver.Methods, context, injectionStubs)) return;

            var injectionsBuilder = new StringBuilder();
            foreach (var injectionStub in injectionStubs)
            {
                injectionsBuilder.AppendLine($"\t\t\tservices.Add(new ServiceDescriptor({injectionStub}), defaultLifetime));");
            }

            string source = SourceTemplate.GenerateSource(outNamespace,
                accessModifier,
                className,
                accessModifier,
                "AddCQRSToolkitGenerated",
                "services",
                "defaultLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient",
                injectionsBuilder.ToString(), false);

            context.AddSource("CQRSToolkit.DependencyInjection.Generator.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new CQRSToolkitSyntaxReceiver());
        }

        #region Private Methods
        private List<string> GetInjections(IEnumerable<ClassDeclarationSyntax> classes, Compilation compilation)
        {
            var injections = new List<string>();
            foreach (var target in classes)
            {
                var model = compilation.GetSemanticModel(target.SyntaxTree);
                if (!(model.GetDeclaredSymbol(target) is INamedTypeSymbol info)
                    || info.IsValueType || info.IsAbstract || info.IsUnboundGenericType || info.AllInterfaces.Length == 0) continue;

                foreach (var candidate in info.AllInterfaces)
                {
                    var constructedName = candidate.ConstructedFrom.ToString();
                    if (constructedName == "CQRSToolkit.IQueryHandler<TQuery, TResponse>"
                    || constructedName == "CQRSToolkit.ICommandHandler<TCommand>"
                    || constructedName == "ICommandResponseHandler<TCommand, TResponse>")
                    {
                        injections.Add($"typeof({candidate}), typeof({info}");
                    }
                }
            }

            return injections;
        }

        private bool TryGenerateMethods(IEnumerable<MethodDeclarationSyntax> methods, GeneratorExecutionContext context, IEnumerable<string> injectionStubs)
        {
            bool hasMethod = false;

            foreach (var method in methods)
            {
                var model = context.Compilation.GetSemanticModel(method.SyntaxTree);

                #region Validate Method Declaration
                if (!(model.GetDeclaredSymbol(method) is IMethodSymbol info)) continue;

                // Check if ServiceInjectionPointAttribute is present on method
                bool isTarget = false;
                foreach (var attribute in info.GetAttributes())
                {
                    if (attribute.ToString() == "CQRSToolkit.DependencyInjection.Generator.Attributes.ServiceInjectionPointAttribute")
                    {
                        isTarget = true;
                        break;
                    }
                }
                if (!isTarget) continue;

                if (!info.IsPartialDefinition)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "CQRSDI0001",
                            "Method must be partial",
                            "Method {0} must be an unimplemented \"partial\" method to use ServiceInjectionPointAttribute.",
                            "CQRSToolkit.Generator",
                            DiagnosticSeverity.Warning,
                            true), info.Locations.FirstOrDefault(), info.Name));
                    continue;
                }

                if (info.PartialImplementationPart != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "CQRSDI0002",
                            "Method can not have an implementation",
                            "Method {0} must not have an implementation provided to use ServiceInjectionPointAttribute.",
                            "CQRSToolkit.Generator",
                            DiagnosticSeverity.Warning,
                            true), info.PartialImplementationPart.Locations.FirstOrDefault(), info.Name));
                    continue;
                }

                if (!info.IsExtensionMethod)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "CQRSDI0003",
                            "Must be an extension method",
                            "Method {0} must be an extension method to use ServiceInjectionPointAttribute.",
                            "CQRSToolkit.Generator",
                            DiagnosticSeverity.Warning,
                            true), info.Locations.FirstOrDefault(), info.Name));
                    continue;
                }
                
                // These shouldn't be possible on an extension method but checking just in case.
                if (info.IsVirtual || info.IsAbstract || info.IsAsync) continue;

                if (info.Parameters.Length != 2)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "CQRSDI0004",
                            "Invalid parameter count",
                            "Method {0} signature must match (this IServiceCollection, ServiceLifetime) to use ServiceInjectionPointAttribute.",
                            "CQRSToolkit.Generator",
                            DiagnosticSeverity.Warning,
                            true), info.Locations.FirstOrDefault(), info.Name));
                    continue;
                }
                
                if (info.Parameters[0].ToString() != "Microsoft.Extensions.DependencyInjection.IServiceCollection")
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "CQRSDI0005",
                            "Invalid parameter type",
                            "Parameter \"{0}\" must be of type \"IServiceCollection\" to use ServiceInjectionPointAttribute.",
                            "CQRSToolkit.Generator",
                            DiagnosticSeverity.Warning,
                            true), info.Parameters[0].Locations.FirstOrDefault(), info.Parameters[0].Name));
                    continue;
                }

                if (info.Parameters[1].ToString() != "Microsoft.Extensions.DependencyInjection.ServiceLifetime")
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "CQRSDI0006",
                            "Invalid parameter type",
                            "Parameter \"{0}\" must be of type \"ServiceLifetime\" to use ServiceInjectionPointAttribute.",
                            "CQRSToolkit.Generator",
                            DiagnosticSeverity.Warning,
                            true), info.Parameters[1].Locations.FirstOrDefault(), info.Parameters[1].Name));
                    continue;
                }

                if (info.ReturnType.ToString() != "Microsoft.Extensions.DependencyInjection.IServiceCollection")
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                       new DiagnosticDescriptor(
                           "CQRSDI0006",
                           "Invalid parameter type",
                           "Return type must be \"IServiceCollection\" to use ServiceInjectionPointAttribute.",
                           "CQRSToolkit.Generator",
                           DiagnosticSeverity.Warning,
                           true), info.Locations.FirstOrDefault()));
                    continue;
                }

                // Invalid extension method access levels. Compiler should already show a good error for this.
                // This check may not actually even be needed.
                switch (info.DeclaredAccessibility)
                {
                    case Accessibility.ProtectedAndInternal:
                    case Accessibility.Protected:
                    case Accessibility.ProtectedOrInternal:
                        continue;
                }
                #endregion

                hasMethod = true;
                ProcessMethod(context, info, injectionStubs);
            }

            return hasMethod;
        }

        private void ProcessMethod(GeneratorExecutionContext context, IMethodSymbol method, IEnumerable<string> injectionStubs)
        {
            string methodModifier;
            switch (method.DeclaredAccessibility)
            {
                case Accessibility.Private:
                    methodModifier = "private";
                    break;
                case Accessibility.Internal:
                    methodModifier = "internal";
                    break;
                case Accessibility.Public:
                    methodModifier = "public";
                    break;
                default:
                    methodModifier = "";
                    break;
            }

            string classAccessModifier;
            switch(method.ContainingType.DeclaredAccessibility)
            {
                case Accessibility.Internal:
                    classAccessModifier = "internal";
                    break;
                case Accessibility.Public:
                    classAccessModifier = "public";
                    break;
                default:
                    classAccessModifier = "";
                    break;
            };

            string outNamespace = method.ContainingNamespace.ToString();
            string className = method.ContainingType.Name;
            string methodName = method.Name;
            string servicesParam = method.Parameters[0].Name;
            string lifetimeParam = method.Parameters[1].Name;

            var injectionsBuilder = new StringBuilder();
            foreach (var injectionStub in injectionStubs)
            {
                injectionsBuilder.AppendLine($"\t\t\t{servicesParam}.Add(new ServiceDescriptor({injectionStub}), {lifetimeParam}));");
            }

            string source = SourceTemplate.GenerateSource(outNamespace, classAccessModifier, className,
                methodModifier, methodName, servicesParam, lifetimeParam, injectionsBuilder.ToString());

            context.AddSource($"{outNamespace}.{className}.{methodName}.g.cs", source);
        }

        private string GetAccessModifier(AnalyzerConfigOptions options)
        {
            string accessModifier = "internal";

            options.TryGetValue("build_property.CQRSToolkit_DIGen_AccessModifier", out var msBuildAccessModifier);
            if (msBuildAccessModifier != null && msBuildAccessModifier.Trim() != "")
            {
                accessModifier = msBuildAccessModifier;
            }

            options.TryGetValue("CQRSToolkit_DIGen_AccessModifier", out var editorConfigAccessModifier);
            if (editorConfigAccessModifier != null && editorConfigAccessModifier.Trim() != "")
            {
                accessModifier = editorConfigAccessModifier;
            }

            return accessModifier;
        }

        private string GetClassName(AnalyzerConfigOptions options)
        {
            string className = "CQRSServiceExtensions";

            options.TryGetValue("build_property.CQRSToolkit_DIGen_ClassName", out var msBuildClassName);
            if (msBuildClassName != null && msBuildClassName.Trim() != "")
            {
                className = msBuildClassName;
            }

            options.TryGetValue("CQRSToolkit_DIGen_ClassName", out var editorConfigClassName);
            if (editorConfigClassName != null && editorConfigClassName.Trim() != "")
            {
                className = editorConfigClassName;
            }

            return className;
        }

        private string GetNamespace(AnalyzerConfigOptions options)
        {
            string outNamespace = "CQRSToolkit.DependencyInjection";

            options.TryGetValue("build_property.CQRSToolkit_DIGen_Namespace", out var msBuildNamespace);
            if (msBuildNamespace != null && msBuildNamespace.Trim() != "")
            {
                outNamespace = msBuildNamespace;
            }

            options.TryGetValue("build_property.CQRSToolkit_DIGen_Namespace", out var editorConfigNamespace);
            if (editorConfigNamespace != null && editorConfigNamespace.Trim() != "")
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