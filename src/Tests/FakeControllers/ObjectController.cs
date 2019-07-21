using System;

namespace Tests.FakeControllers
{
    public class ObjectController
    {
        public ObjectController()
        {
            OnAction = () => string.Empty;
        }
        public Func<object> OnAction { get; set; }
        /// <summary>
        /// ActionHelp
        /// </summary>
        public object Action() { return OnAction(); }
    }
}

