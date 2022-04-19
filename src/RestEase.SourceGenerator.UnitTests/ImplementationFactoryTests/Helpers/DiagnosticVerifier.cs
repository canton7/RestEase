using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using RestEase.Implementation;
using Xunit;

namespace RestEase.UnitTests.ImplementationFactoryTests.Helpers
{
    public static class DiagnosticVerifier
    {
        public static void VerifyDiagnostics(
            IEnumerable<Diagnostic> diagnostics,
            DiagnosticResult[] expected,
            int lineOffset)
        {
            var sortedDiagnostics = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToList();
            VerifyDiagnosticResults(sortedDiagnostics, expected, lineOffset);
        }

        private static void VerifyDiagnosticResults(
            List<Diagnostic> actualResults,
            DiagnosticResult[] expectedResults,
            int lineOffset)
        {
            int expectedCount = expectedResults.Length;
            int actualCount = actualResults.Count;

            if (expectedCount != actualCount)
            {
                string diagnosticsOutput = actualResults.Any() ? FormatDiagnostics(lineOffset, actualResults.ToArray()) : "    NONE.";

                Assert.True(false,
                    string.Format("Mismatch between number of diagnostics returned, expected \"{0}\" actual \"{1}\"\r\n\r\nDiagnostics:\r\n{2}\r\n", expectedCount, actualCount, diagnosticsOutput));
            }

            for (int i = 0; i < expectedResults.Length; i++)
            {
                var actual = actualResults[i];
                var expected = expectedResults[i];

                if (expected.Locations.Count == 0)
                {
                    if (actual.Location != Location.None)
                    {
                        Assert.True(false,
                            string.Format("Expected:\nA project diagnostic with No location\nActual:\n{0}",
                            FormatDiagnostics(lineOffset, actual)));
                    }
                }
                else
                {
                    VerifyDiagnosticLocation(actual, actual.Location, expected.Locations.First(), lineOffset);
                    var additionalLocations = actual.AdditionalLocations.ToArray();

                    if (additionalLocations.Length != expected.Locations.Count - 1)
                    {
                        Assert.True(false,
                            string.Format("Expected {0} additional locations but got {1} ({2}) for Diagnostic:\r\n{3}\r\n",
                                expected.Locations.Count - 1, additionalLocations.Length,
                                string.Join(", ", additionalLocations.Select(x =>
                                {
                                    var start = x.GetLineSpan().StartLinePosition;
                                    return $"({start.Line + 1 - lineOffset},{start.Character + 1})";
                                })),
                                FormatDiagnostics(lineOffset, actual)));
                    }

                    for (int j = 0; j < additionalLocations.Length; ++j)
                    {
                        VerifyDiagnosticLocation(actual, additionalLocations[j], expected.Locations[j + 1], lineOffset);
                    }
                }

                if (actual.Id != expected.Code.Format())
                {
                    Assert.True(false,
                        string.Format("Expected diagnostic id to be \"{0}\" ({1}) was \"{2}\"\r\n\r\nDiagnostic:\r\n{3}\r\n",
                            expected.Code.Format(), expected.Code, actual.Id, FormatDiagnostics(lineOffset, actual)));
                }

                var expectedSeverity = expected.IsError ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning;
                if (actual.Severity != expectedSeverity)
                {
                    Assert.True(false,
                        string.Format("Expected diagnostic severity to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n{2}\r\n",
                            expectedSeverity, actual.Severity, FormatDiagnostics(lineOffset, actual)));
                }

                string squiggledText = GetSquiggledText(actual);
                if (squiggledText != expected.SquiggledText)
                {
                    Assert.True(false,
                        string.Format("Expected squiggled text to be \"{0}\", was \"{1}\"\r\n\r\nDiagnostic:\r\n{2}\r\n",
                        expected.SquiggledText, squiggledText, FormatDiagnostics(lineOffset, actual)));
                }

                //if (actual.GetMessage() != expected.Message)
                //{
                //    Assert.True(false,
                //        string.Format("Expected diagnostic message to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                //            expected.Message, actual.GetMessage(), FormatDiagnostics(actual)));
                //}
            }
        }

        /// <summary>
        /// Helper method to VerifyDiagnosticResult that checks the location of a diagnostic and compares it with the location in the expected DiagnosticResult.
        /// </summary>
        private static void VerifyDiagnosticLocation(
            Diagnostic diagnostic,
            Location actual,
            DiagnosticResultLocation expected,
            int lineOffset)
        {
            var actualSpan = actual.GetLineSpan();

            //Assert.True(actualSpan.Path == expected.Path || (actualSpan.Path != null && actualSpan.Path.Contains("Test0.") && expected.Path.Contains("Test.")),
            //    string.Format("Expected diagnostic to be in file \"{0}\" was actually in file \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
            //        expected.Path, actualSpan.Path, FormatDiagnostics(diagnostic)));

            var actualLinePosition = actualSpan.StartLinePosition;

            // Only check line position if there is an actual line in the real diagnostic
            if (actualLinePosition.Line > 0)
            {
                if (actualLinePosition.Line + 1 - lineOffset != expected.Line)
                {
                    Assert.True(false,
                        string.Format("Expected diagnostic to be on line \"{0}\" was actually on line \"{1}\"\r\n\r\nDiagnostic:\r\n{2}\r\n",
                            expected.Line, actualLinePosition.Line + 1 - lineOffset, FormatDiagnostics(lineOffset, diagnostic)));
                }
            }

            // Only check column position if there is an actual column position in the real diagnostic
            if (actualLinePosition.Character > 0)
            {
                if (actualLinePosition.Character + 1 != expected.Column)
                {
                    Assert.True(false,
                        string.Format("Expected diagnostic to start at column \"{0}\" was actually at column \"{1}\"\r\n\r\nDiagnostic:\r\n{2}\r\n",
                            expected.Column, actualLinePosition.Character + 1, FormatDiagnostics(lineOffset, diagnostic)));
                }
            }
        }

        private static string FormatDiagnostics(int lineOffset, params Diagnostic[] diagnostics)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < diagnostics.Length; ++i)
            {
                var location = diagnostics[i].Location;

                int line = 0;
                int col = 0;
                if (location != Location.None)
                {
                    var mappedSpan = location.GetMappedLineSpan().Span;
                    line = mappedSpan.Start.Line + 1 - lineOffset;
                    col = mappedSpan.Start.Character + 1;
                }

                builder.AppendFormat("// ({0},{1}): {2} {3}: {4}",
                    line,
                    col,
                    diagnostics[i].Severity,
                    diagnostics[i].Id,
                    diagnostics[i].GetMessage(null)).AppendLine();

                string squiggledText = "";
                if (location == Location.None)
                {
                    builder.AppendFormat("// {0}.{1}", diagnostics[i].Descriptor.Title, diagnostics[i].Id).AppendLine();
                }
                else
                {
                    Assert.True(location.IsInSource,
                        $"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata: {diagnostics[i]}\r\n");

                    squiggledText = GetSquiggledText(diagnostics[i]);
                    builder.AppendFormat("// {0}", squiggledText).AppendLine();
                }

                var code = (DiagnosticCode)int.Parse(diagnostics[i].Id["REST".Length..]);
                builder.AppendFormat("Diagnostic(DiagnosticCode.{0}, @\"{1}\")",
                    code.ToString(),
                    squiggledText.Replace("\"", "\"\""));
                if (location != Location.None)
                {
                    builder.AppendFormat(".WithLocation({0}, {1})",
                        line,
                        col);
                }

                builder.AppendLine().AppendLine();
            }
            return builder.ToString();
        }

        private static string GetSquiggledText(Diagnostic diagnostic)
        {
            return diagnostic.Location == Location.None
                ? ""
                : diagnostic.Location.SourceTree.GetText().ToString(diagnostic.Location.SourceSpan);
        }
    }
}
