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
                    
                    sourceBuilder.AppendLine(
$@"
            events.RegisterMessage<{reg.ViewModelTypeName}, {reg.MessageTypeName}>
                    ((v, a) => v.{reg.EventName} += a, (v, a) => v.{reg.EventName} -= a);
");
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
                                                                           select reg.MessageNamespace)
                                        .Union(from reg in Registrations
                                               select reg.ViewModelNamespace)
                                        .Distinct()
                                        .OrderBy(x => x)
                                        .Select(x => $"using {x};"));
                                        

            public IList<EventRegistration> Registrations { get; } = new List<EventRegistration>(new EventRegistration[]{
                    new EventRegistration{ViewModelTypeName="SecondViewModel",ViewModelNamespace="MvvmNavigation.ViewModels", MessageTypeName="CloseMessage",MessageNamespace="MvvmNavigation.Messages", EventName="ViewModelDone"}
                }); 

            public ClassDeclarationSyntax AppServiceClass { get; set; }
            public IList<EventDeclarationSyntax> CandidateEvents { get; } = new List<EventDeclarationSyntax>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                try
                {
                    Debug.WriteLine(syntaxNode.ToString());

                    if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
                    {
                        var classAttribute = classDeclarationSyntax.AttributeLists.FirstOrDefault()?.Attributes.FirstOrDefault(attrib => (attrib.Name as IdentifierNameSyntax).Identifier.Text + "Attribute" == typeof(ApplicationServiceAttribute).Name);
                        if (classAttribute != null)
                        {
                            //if (!Debugger.IsAttached)
                            //{
                            //    Debugger.Launch();
                            //}
                            //else
                            //{
                            //    Debugger.Break();
                            //}

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
            public string MessageNamespace { get; set; }
            public string EventName { get; set; }
        }
    }
}
