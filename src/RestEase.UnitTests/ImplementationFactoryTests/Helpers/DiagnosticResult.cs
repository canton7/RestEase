using System;
using System.Collections.Generic;
using RestEase.Implementation;

namespace RestEase.UnitTests.ImplementationFactoryTests.Helpers
{
    public class DiagnosticResult
    {
        public DiagnosticCode Code { get; }
        public string SquiggledText { get; }
        public bool IsError { get; set; } = true;
        public List<DiagnosticResultLocation> Locations { get; } = new List<DiagnosticResultLocation>();

        public DiagnosticResult(DiagnosticCode code, string squiggledText)
        {
            this.Code = code;
            this.SquiggledText = squiggledText;
        }

        public DiagnosticResult WithLocation(int line, int column)
        {
            this.Locations.Add(new DiagnosticResultLocation(line, column));
            return this;
        }

    }

    public readonly struct DiagnosticResultLocation
    {
        public int Line { get; }
        public int Column { get; }

        public DiagnosticResultLocation(int line, int column)
        {
            if (column < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(column), "column must be >= -1");
            }

            this.Line = line;
            this.Column = column;
        }
    }
}
