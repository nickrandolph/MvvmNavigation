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
    public class PageViewModelGenerator : ISourceGenerator
    {

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                // the generator infrastructure will create a receiver and populate it
                // we can retrieve the populated instance via the context
                var syntaxReceiver = context.SyntaxReceiver as SyntaxReceiver;

                var compilation = context.Compilation;

                //var evt = syntaxReceiver.Events.FirstOrDefault();

                foreach (var reg in syntaxReceiver.Registrations)
                {
                    var messageAttribute = reg.MappingSyntax;

                    //var messageAttribute = evt.AttributeLists
                    //    .FirstOrDefault()?.Attributes
                    //    .FirstOrDefault(attrib => (attrib.Name as IdentifierNameSyntax).Identifier.Text + "Attribute" == typeof(EventMessageAttribute).Name);

                    var allNodes = messageAttribute.DescendantNodes();

                    var syntaxTree = messageAttribute.SyntaxTree;
                    var model = compilation.GetSemanticModel(syntaxTree);

                    var node = allNodes
                        .Where(x => x.Kind() == SyntaxKind.IdentifierName)
                        .FirstOrDefault() as IdentifierNameSyntax;


                    var symbol = model.GetSymbolInfo(node).Symbol as ITypeSymbol;

                    var typeName = symbol.Name;
                    var ns = symbol.ContainingNamespace;
                    var namespaceName = string.Empty;
                    while (ns != null)
                    {
                        namespaceName = ns.Name + (namespaceName == string.Empty || ns.Name == string.Empty ? "" : ".") + namespaceName;
                        ns = ns.ContainingNamespace;
                    }
                    reg.ViewModelTypeName = typeName;
                    reg.ViewModelNamespace = namespaceName;
                }

                if (!string.IsNullOrWhiteSpace(syntaxReceiver?.AppTypeName))
                {


                    var sourceBuilder = new StringBuilder($@"
                using BuildIt.Navigation;
                {syntaxReceiver.Namespaces}
                using System;
                namespace {syntaxReceiver.AppNamespace}
                {{
                    public partial class {syntaxReceiver.AppTypeName}
                    {{
                        partial void {syntaxReceiver.RegistrationPartialMethodName}(WindowsViewModelToViewMapping mappings)
                        {{
                ");

                    foreach (var reg in syntaxReceiver.Registrations)
                    {
                        sourceBuilder.AppendLine(
                            $@"
                                        mappings.RegisterForNavigation<{reg.PageTypeName}, {reg.ViewModelTypeName}>();
                                        ");
                    }

                    // finish creating the source to inject
                    sourceBuilder.Append(@"
                        }
                    }
                }");

                    // inject the created source into the users compilation
                    context.AddSource("App.generated", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
                }

                foreach (var reg in syntaxReceiver.Registrations)
                {
                    var vmInit = string.IsNullOrWhiteSpace(reg.ViewModelInitMethodName) ? null :
$@"
 partial void {reg.ViewModelInitMethodName}()
        {{
            var sp = (Application.Current as INavigationApplication)?.AppService?.Services;
            var viewModelType=sp.GetService<IViewModelToViewMapping>().ViewModelFromView<{reg.PageTypeName}>();
            var vm = sp.GetService(viewModelType);
            DataContext = vm;
        }}
";

                    var regSourceBuilder = new StringBuilder($@"
using BuildIt.Navigation;
using {reg.ViewModelNamespace};
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
namespace {reg.PageNamespace}
{{
    public partial class {reg.PageTypeName}
    {{
        //public {reg.ViewModelTypeName} ViewModel2 => DataContext as {reg.ViewModelTypeName};

        {vmInit}
    }}
}}");

                    // inject the created source into the users compilation
                    context.AddSource($"{reg.PageTypeName}.generated", SourceText.From(regSourceBuilder.ToString(), Encoding.UTF8));

                }
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
            public string AppTypeName { get; set; }
            public string AppNamespace { get; set; }

            public string RegistrationPartialMethodName { get; set; }

            public string Namespaces => string.Join(Environment.NewLine, (from reg in Registrations
                                                                          where reg.PageNamespace!=null
                                                                           select reg.PageNamespace)
                                        .Union(from reg in Registrations
                                               where reg.ViewModelNamespace!=null
                                               select reg.ViewModelNamespace)
                                        .Distinct()
                                        .OrderBy(x => x)
                                        .Select(x => $"using {x};"));


            public IList<PageViewModelRegistration> Registrations { get; } = new List<PageViewModelRegistration>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                try
                {
                    Debug.WriteLine(syntaxNode.ToString());

                    if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
                    {
                        var classAttribute = classDeclarationSyntax.AttributeLists.FirstOrDefault()?.Attributes.FirstOrDefault(attrib => (attrib.Name as IdentifierNameSyntax)?.Identifier.Text + "Attribute" == typeof(ViewModelMappingRegisterAttribute).Name);
                        var viewModelClassAttribute = classDeclarationSyntax.AttributeLists.FirstOrDefault()?.Attributes.FirstOrDefault(attrib => (attrib.Name as IdentifierNameSyntax)?.Identifier.Text + "Attribute" == typeof(ViewModelAttribute).Name);
                        if (classAttribute != null)
                        {
                            AppTypeName = classDeclarationSyntax.Identifier.ValueText;

                            var namespaceSyntax = classDeclarationSyntax.Parent as NamespaceDeclarationSyntax;
                            if (namespaceSyntax != null)
                            {
                                AppNamespace = namespaceSyntax.Name.ToString();
                            }


                            var firstArgument = classAttribute.ArgumentList.Arguments.FirstOrDefault();
                            var identifier = firstArgument.Expression
                                                .DescendantNodes()
                                                .Where(x => x.Kind() == SyntaxKind.IdentifierName)
                                                .Skip(1)
                                                .FirstOrDefault() as IdentifierNameSyntax;
                            RegistrationPartialMethodName = identifier.Identifier.ValueText;

                        }
                        else if(viewModelClassAttribute!=null)
                        {
                            var pageTypeName = classDeclarationSyntax.Identifier.ValueText;
                            var pageNamespace = "";
                            var namespaceSyntax = classDeclarationSyntax.Parent as NamespaceDeclarationSyntax;
                            if (namespaceSyntax != null)
                            {
                                pageNamespace = namespaceSyntax.Name.ToString();
                            }
                            var firstArgument = viewModelClassAttribute.ArgumentList.Arguments.FirstOrDefault();

                            var secondArgument = viewModelClassAttribute.ArgumentList.Arguments.Skip(1).FirstOrDefault();
                            var identifier = secondArgument?.Expression
                                                ?.DescendantNodes()
                                                .Where(x => x.Kind() == SyntaxKind.IdentifierName)
                                                .Skip(1)
                                                .FirstOrDefault() as IdentifierNameSyntax;

                            Registrations.Add(new PageViewModelRegistration()
                            {
                                ViewModelInitMethodName= identifier?.Identifier.ValueText,
                                MappingSyntax = firstArgument,
                                PageTypeName=pageTypeName,
                                PageNamespace=pageNamespace
                            });

                        }
                    }
                }
                catch(Exception ex)
                {

                }
            }
        }

        private class PageViewModelRegistration
        {
            public string PageTypeName { get; set; }
            public string PageNamespace { get; set; }
            public string ViewModelTypeName { get; set; }
            public string ViewModelNamespace { get; set; }
            public string ViewModelInitMethodName { get; set; }
            public AttributeArgumentSyntax MappingSyntax { get; set; }
        }
    }
}
