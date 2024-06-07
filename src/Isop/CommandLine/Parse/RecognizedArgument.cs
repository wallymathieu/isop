namespace Isop.CommandLine.Parse
{
    public record RecognizedArgument(Argument Argument, 
        int[] Index, 
        string RawArgument, 
        string? Value = null,
        bool InferredOrdinal = false)
    {
        /// <summary>
        /// the matched value if any, for instance the "value" of the expression "--argument value"
        /// </summary>
        public string? Value { get; } = Value;
        /// <summary>
        /// the "argument" of the expression "--argument"
        /// </summary>
        public string RawArgument { get; } = RawArgument;
   }
}

