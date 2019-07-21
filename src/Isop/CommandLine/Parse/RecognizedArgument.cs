namespace Isop.CommandLine.Parse
{
    public class RecognizedArgument
    {
        public int[] Index { get; }

        /// <summary>
        /// the matched value if any, for instance the "value" of the expression "--argument value"
        /// </summary>
        public string Value { get; }
        public Argument Argument { get; }
        /// <summary>
        /// the "argument" of the expression "--argument"
        /// </summary>
        public string RawArgument { get; }

        public bool InferredOrdinal { get; }

        public RecognizedArgument(Argument argument, int[] index, string rawArgument, string value = null, bool inferredOrdinal =false)
        {
            Index = index;
            Value = value;
            Argument = argument;
            RawArgument = rawArgument;
            InferredOrdinal = inferredOrdinal;
        }
    }
}

