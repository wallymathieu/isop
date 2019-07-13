using Isop.Api;

namespace Isop
{
    /// <summary>
    /// Extensions of <see cref="Builder"/>
    /// </summary>
    public static class BuilderExtensions
    {
        public static Api.Builder Recognize<T>(this Api.Builder build, bool ignoreGlobalUnMatchedParameters = false)
        {
            return build.Recognize(typeof(T), ignoreGlobalUnMatchedParameters);
        }
    }
}

