using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Plus.AutoApi;
using Plus.AutoApi.Core;

namespace Plus.AutoApiGenerator
{
    [Generator]
    public class ServiceGenerator : ISourceGenerator
    {
        private const string NATIVE_SERVICES_TXT_ADDITIONAL_FILE_NAME = "Services.txt";
        private static readonly char[] ZeroWhiteSpace = new char[]
        {
            '\uFEFF', // ZERO WIDTH NO-BREAK SPACE (U+FEFF)
            '\u200B', // ZERO WIDTH SPACE (U+200B)
        };
        public void Initialize(GeneratorInitializationContext context)
        {

        }

        public void Execute(GeneratorExecutionContext context)
        {
            Debugger.Launch();
            var nativeMethodsTxtFiles = context.AdditionalFiles
                .Where(af => string.Equals(Path.GetFileName(af.Path), NATIVE_SERVICES_TXT_ADDITIONAL_FILE_NAME, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!nativeMethodsTxtFiles.Any())
            {
                return;
            }

            var parseOptions = (CSharpParseOptions)context.ParseOptions;

            foreach (AdditionalText nativeMethodsTxtFile in nativeMethodsTxtFiles)
            {
                var nativeMethodsTxt = nativeMethodsTxtFile.GetText(context.CancellationToken);
                if (nativeMethodsTxt is null)
                {
                    return;
                }

                foreach (TextLine line in nativeMethodsTxt.Lines)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    string name = line.ToString();
                    if (string.IsNullOrWhiteSpace(name) || name.StartsWith("//", StringComparison.InvariantCulture))
                    {
                        continue;
                    }

                    name = name.Trim().Trim(ZeroWhiteSpace);
                    var location = Location.Create(nativeMethodsTxtFile.Path, line.Span, nativeMethodsTxt.Lines.GetLinePositionSpan(line.Span));

                    context.AddSource(name, $"public class {name}{{}}");
                }
            }
        }
    }
}
