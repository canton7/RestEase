using Microsoft.CodeAnalysis;

namespace RestEase.Implementation.Analysis
{
    internal partial class EventModel
    {
        public IEventSymbol EventSymbol { get; }

        public EventModel(IEventSymbol eventSymbol)
        {
            this.EventSymbol = eventSymbol;
        }
    }
}