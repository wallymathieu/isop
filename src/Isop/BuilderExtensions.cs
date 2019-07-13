using Isop.Api;

namespace Isop
{
    /// <summary>
    /// Extensions of <see cref="Build"/>
    /// </summary>
    public static class BuilderExtensions
    {
        public static Builder Recognize<T>(this Builder build, bool ignoreGlobalUnMatchedParameters = false)
        {
            return build.Recognize(typeof(T), ignoreGlobalUnMatchedParameters);
        }
    }
}

