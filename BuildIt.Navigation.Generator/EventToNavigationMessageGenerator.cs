using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mime;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace BuildIt.Navigation.Generator
{
    [Generator]
    public class EventToNavigationMessageGenerator : ISourceGenerator
    {

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                // the generator infrastructure will create a receiver and populate it
                // we can retrieve the populated instance via the context
                var syntaxReceiver = context.SyntaxReceiver as SyntaxReceiver;
                if (string.IsNullOrWhiteSpace( syntaxReceiver?.AppServiceTypeName))
                {
                    return;
                }


                var compilation = context.Compilation;

                //var evt = syntaxReceiver.Events.FirstOrDefault();

                foreach (var reg in syntaxReceiver.Registrations)
                {
                    var evt = reg.EventSyntax;

                    var messageAttribute = evt.AttributeLists
                        .FirstOrDefault()?.Attributes
                        .FirstOrDefault(attrib => (attrib.Name as IdentifierNameSyntax).Identifier.Text + "Attribute" == typeof(EventMessageAttribute).Name);

                    var allNodes = messageAttribute.ArgumentList.DescendantNodes();

                    var genericType = allNodes
                        .Where(x => x.Kind() == SyntaxKind.GenericName)
                        .FirstOrDefault() as GenericNameSyntax;

                    var syntaxTree = evt.SyntaxTree;
                    var model = compilation.GetSemanticModel(syntaxTree);

                    if (genericType == null)
                    {
                        var node = allNodes
                            .Where(x => x.Kind() == SyntaxKind.IdentifierName)
                            .FirstOrDefault() as IdentifierNameSyntax;


                        var symbol = model.GetSymbolInfo(node).Symbol as ITypeSymbol;

                        //var typeName = symbol.Name;
                        //var ns = symbol.ContainingNamespace;
                        //var namespaceName = string.Empty;
                        //while (ns != null)
                        //{
                        //    namespaceName = ns.Name + (namespaceName == string.Empty || ns.Name == string.Empty ? "" : ".") + namespaceName;
                        //    ns = ns.ContainingNamespace;
                        //}
                        reg.MessageTypeName = symbol.ToString();
                        //reg.MessageNamespace = namespaceName;
                    }
                    else
                    {
                        var symbol = model.GetSymbolInfo(genericType).Symbol as ITypeSymbol;
                        reg.MessageTypeName = symbol.ToString();

                        var genericTypeParameter = genericType
                            .DescendantNodes()
                        .Where(x => x.Kind() == SyntaxKind.IdentifierName)
                        .FirstOrDefault() as IdentifierNameSyntax;
                        symbol = model.GetSymbolInfo(genericTypeParameter).Symbol as ITypeSymbol;
                        reg.ParameterType = symbol.ToString();

                        var ns = symbol.ContainingNamespace;
                        var namespaceName = string.Empty;
                        while (ns != null)
                        {
                            namespaceName = ns.Name + (namespaceName == string.Empty || ns.Name == string.Empty ? "" : ".") + namespaceName;
                            ns = ns.ContainingNamespace;
                        }
                        reg.ParameterNamespace = namespaceName;

                        var messageParamater = allNodes
                        .Where(x => x.Kind() == SyntaxKind.SimpleMemberAccessExpression)
                        .FirstOrDefault() as MemberAccessExpressionSyntax;
                        reg.ParameterValueAsString = messageParamater.ToString();
                    }


                    var classDec = evt.Parent as ClassDeclarationSyntax;

                    //symbol = model.GetSymbolInfo(classDec).Symbol as ITypeSymbol;
                    var vmType = classDec.Identifier.ValueText;// symbol.Name;
                    var nsDec = classDec.Parent as NamespaceDeclarationSyntax;

                    var vmNamespace = nsDec.Name.ToString();

                    reg.ViewModelTypeName = vmType;
                    reg.ViewModelNamespace = vmNamespace;
                }

                var sourceBuilder = new StringBuilder($@"
using BuildIt.Navigation;
using BuildIt.Navigation.Messages;
using Microsoft.Extensions.DependencyInjection;
{syntaxReceiver.Namespaces}
using System;
namespace {syntaxReceiver.AppServiceNamespace}
{{
    public partial class {syntaxReceiver.AppServiceTypeName}
    {{
        

        partial void {syntaxReceiver.RegistrationPartialMethodName}(NavigationEvents events)
        {{
");

                foreach (var reg in syntaxReceiver.Registrations)
                {
                    if (string.IsNullOrWhiteSpace(reg.ParameterValueAsString))
                    {
                        sourceBuilder.AppendLine(
                            $@"
                                    events.RegisterMessage<{reg.ViewModelTypeName}, {reg.MessageTypeName}>
                                            ((v, a) => v.{reg.EventName} += a, (v, a) => v.{reg.EventName} -= a);
                            ");
                    }
                    else
                    {
                        sourceBuilder.AppendLine(
                            $@"
                                    events.RegisterMessageWithParameter<{reg.ViewModelTypeName}, {reg.MessageTypeName}, {reg.ParameterType}>
                                            ((v, a) => v.{reg.EventName} += a, (v, a) => v.{reg.EventName} -= a, {reg.ParameterValueAsString});
                            ");
                        //.RegisterMessageWithParameter<MainViewModel, CompletedWithStatusMessage<CompletionStates>, CompletionStates>
                        //    ((v, a) => v.ViewModelAlsoDone += a, (v, a) => v.ViewModelAlsoDone -= a, CompletionStates.Two);
                    }
                }

                // finish creating the source to inject
                sourceBuilder.Append(@"
        }
    }
}");

                // inject the created source into the users compilation
                context.AddSource("MvvmApplicationServices.generated.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            }
            catch(Exception ex)
            {

            }

        }


        public void Initialize(GeneratorInitializationContext context)
        {
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public string AppServiceTypeName { get; set; }// => "MvvmApplicationServices";
            public string AppServiceNamespace { get; set; }// => "MvvmNavigation";

            public string RegistrationPartialMethodName { get; set; }// = "RegisterEvents";

            public string Namespaces => string.Join(Environment.NewLine, (from reg in Registrations
                                                                          where reg.ParameterNamespace!=null
                                                                           select reg.ParameterNamespace)
                                        .Union(from reg in Registrations
                                               where reg.ViewModelNamespace!=null
                                               select reg.ViewModelNamespace)
                                        .Distinct()
                                        .OrderBy(x => x)
                                        .Select(x => $"using {x};"));


            public IList<EventRegistration> Registrations { get; } = new List<EventRegistration>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                try
                {
                    Debug.WriteLine(syntaxNode.ToString());

                    if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
                    {
                        var classAttribute = classDeclarationSyntax.AttributeLists.FirstOrDefault()?.Attributes.FirstOrDefault(attrib => (attrib.Name as IdentifierNameSyntax)?.Identifier.Text + "Attribute" == typeof(ApplicationServiceAttribute).Name);
                        if (classAttribute != null)
                        {
                            AppServiceTypeName = classDeclarationSyntax.Identifier.ValueText;

                            var namespaceSyntax = classDeclarationSyntax.Parent as NamespaceDeclarationSyntax;
                            if (namespaceSyntax != null)
                            {
                                AppServiceNamespace = namespaceSyntax.Name.ToString();
                            }


                            var firstArgument = classAttribute.ArgumentList.Arguments.FirstOrDefault();
                            var identifier = firstArgument.Expression
                                                .DescendantNodes()
                                                .Where(x => x.Kind() == SyntaxKind.IdentifierName)
                                                .Skip(1)
                                                .FirstOrDefault() as IdentifierNameSyntax;
                            RegistrationPartialMethodName = identifier.Identifier.ValueText;

                        }
                    }

                    if (syntaxNode is EventFieldDeclarationSyntax eventDeclarationSyntax)
                    {
                        var messageAttribute = eventDeclarationSyntax.AttributeLists.FirstOrDefault()?.Attributes.FirstOrDefault(attrib => (attrib.Name as IdentifierNameSyntax).Identifier.Text + "Attribute" == typeof(EventMessageAttribute).Name);
                        if (messageAttribute != null)
                        {
                            var eventName = eventDeclarationSyntax.Declaration.Variables.FirstOrDefault().Identifier.ValueText;
                            Registrations.Add(new EventRegistration() { 
                                EventSyntax = eventDeclarationSyntax,
                            EventName = eventName
                            });
                            //SemanticModel model = compilation.GetSemanticModel(syntaxTree);
                            //ISymbol symbol = model.GetSymbolInfo(syntaxNode).Symbol;
                            // symbol is probably an ITypeSymbol

                            //if(!Debugger.IsAttached)
                            //{
                            //    Debugger.Launch();
                            //}
                            //else
                            //{
                            //    Debugger.Break();
                            //}
                        }
                    }
                }
                catch(Exception ex)
                {

                }
            }
        }

        private class EventRegistration
        {
            public string ViewModelTypeName { get; set; }
            public string ViewModelNamespace { get; set; }
            public string MessageTypeName { get; set; }
            //public string MessageNamespace { get; set; }
            public string EventName { get; set; }
            public string ParameterType { get; set; }
            public string ParameterNamespace { get; set; }
            public string ParameterValueAsString { get; set; }

            public EventFieldDeclarationSyntax EventSyntax { get; set; }
        }
    }
}
