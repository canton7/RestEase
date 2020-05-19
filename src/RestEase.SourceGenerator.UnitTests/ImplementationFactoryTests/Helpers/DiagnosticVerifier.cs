using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using RestEase.Implementation;
using Xunit;

namespace RestEaseUnitTests.ImplementationFactoryTests.Helpers
{
    public static class DiagnosticVerifier
    {
        public static void VerifyDiagnostics(IEnumerable<Diagnostic> diagnostics, params DiagnosticResult[] expected)
        {
            var sortedDiagnostics = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
            VerifyDiagnosticResults(sortedDiagnostics, expected);
        }

        private static void VerifyDiagnosticResults(IEnumerable<Diagnostic> actualResults, params DiagnosticResult[] expectedResults)
        {
            int expectedCount = expectedResults.Count();
            int actualCount = actualResults.Count();

            if (expectedCount != actualCount)
            {
                string diagnosticsOutput = actualResults.Any() ? FormatDiagnostics(actualResults.ToArray()) : "    NONE.";

                Assert.True(false,
                    string.Format("Mismatch between number of diagnostics returned, expected \"{0}\" actual \"{1}\"\r\n\r\nDiagnostics:\r\n{2}\r\n", expectedCount, actualCount, diagnosticsOutput));
            }

            for (int i = 0; i < expectedResults.Length; i++)
            {
                var actual = actualResults.ElementAt(i);
                var expected = expectedResults[i];

                if (expected.Locations.Count == 0)
                {
                    if (actual.Location != Location.None)
                    {
                        Assert.True(false,
                            string.Format("Expected:\nA project diagnostic with No location\nActual:\n{0}",
                            FormatDiagnostics(actual)));
                    }
                }
                else
                {
                    VerifyDiagnosticLocation(actual, actual.Location, expected.Locations.First());
                    var additionalLocations = actual.AdditionalLocations.ToArray();

                    if (additionalLocations.Length != expected.Locations.Count - 1)
                    {
                        Assert.True(false,
                            string.Format("Expected {0} additional locations but got {1} for Diagnostic:\r\n{2}\r\n",
                                expected.Locations.Count - 1, additionalLocations.Length,
                                FormatDiagnostics(actual)));
                    }

                    for (int j = 0; j < additionalLocations.Length; ++j)
                    {
                        VerifyDiagnosticLocation(actual, additionalLocations[j], expected.Locations[j + 1]);
                    }
                }

                if (actual.Id != expected.Code.Format())
                {
                    Assert.True(false,
                        string.Format("Expected diagnostic id to be \"{0}\" ({1}) was \"{2}\"\r\n\r\nDiagnostic:\r\n{3}\r\n",
                            expected.Code.Format(), expected.Code, actual.Id, FormatDiagnostics(actual)));
                }

                var expectedSeverity = expected.IsError ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning;
                if (actual.Severity != expectedSeverity)
                {
                    Assert.True(false,
                        string.Format("Expected diagnostic severity to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n{2}\r\n",
                            expectedSeverity, actual.Severity, FormatDiagnostics(actual)));
                }

                string squiggledText = GetSquiggledText(actual);
                if (squiggledText != expected.SquiggledText)
                {
                    Assert.True(false,
                        string.Format("Expected squiggled text to be \"{0}\", was \"{1}\"\r\n\r\nDiagnostic:\r\n{2}\r\n",
                        expected.SquiggledText, squiggledText, FormatDiagnostics(actual)));
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
        /// <param name="diagnostic">The diagnostic that was found in the code</param>
        /// <param name="actual">The Location of the Diagnostic found in the code</param>
        /// <param name="expected">The DiagnosticResultLocation that should have been found</param>
        private static void VerifyDiagnosticLocation(Diagnostic diagnostic, Location actual, DiagnosticResultLocation expected)
        {
            var actualSpan = actual.GetLineSpan();

            //Assert.True(actualSpan.Path == expected.Path || (actualSpan.Path != null && actualSpan.Path.Contains("Test0.") && expected.Path.Contains("Test.")),
            //    string.Format("Expected diagnostic to be in file \"{0}\" was actually in file \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
            //        expected.Path, actualSpan.Path, FormatDiagnostics(diagnostic)));

            var actualLinePosition = actualSpan.StartLinePosition;

            // Only check line position if there is an actual line in the real diagnostic
            if (actualLinePosition.Line > 0)
            {
                if (actualLinePosition.Line + 1 != expected.Line)
                {
                    Assert.True(false,
                        string.Format("Expected diagnostic to be on line \"{0}\" was actually on line \"{1}\"\r\n\r\nDiagnostic:\r\n{2}\r\n",
                            expected.Line, actualLinePosition.Line + 1, FormatDiagnostics(diagnostic)));
                }
            }

            // Only check column position if there is an actual column position in the real diagnostic
            if (actualLinePosition.Character > 0)
            {
                if (actualLinePosition.Character + 1 != expected.Column)
                {
                    Assert.True(false,
                        string.Format("Expected diagnostic to start at column \"{0}\" was actually at column \"{1}\"\r\n\r\nDiagnostic:\r\n{2}\r\n",
                            expected.Column, actualLinePosition.Character + 1, FormatDiagnostics(diagnostic)));
                }
            }
        }

        private static string FormatDiagnostics(params Diagnostic[] diagnostics)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < diagnostics.Length; ++i)
            {
                builder.AppendFormat("// {0}", diagnostics[i].ToString()).AppendLine();

                var location = diagnostics[i].Location;
                if (location == Location.None)
                {
                    builder.AppendFormat("// {0}.{1}", diagnostics[i].Descriptor.Title, diagnostics[i].Id);
                }
                else
                {
                    Assert.True(location.IsInSource,
                        $"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata: {diagnostics[i]}\r\n");

                    builder.AppendFormat("// {0}\r\n", GetSquiggledText(diagnostics[i]));
                }

                builder.AppendLine();
            }
            return builder.ToString();
        }

        private static string GetSquiggledText(Diagnostic diagnostic)
        {
            return diagnostic.Location.SourceTree.GetText().ToString(diagnostic.Location.SourceSpan);
        }
    }
}
