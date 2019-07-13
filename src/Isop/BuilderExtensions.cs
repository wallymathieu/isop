using Isop.Api;

namespace Isop
{
    /// <summary>
    /// Extensions of <see cref="Builder"/>
    /// </summary>
    public static class BuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="build"></param>
        /// <param name="ignoreGlobalUnMatchedParameters"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Builder Recognize<T>(this Builder build, bool ignoreGlobalUnMatchedParameters = false)
        {
            return build.Recognize(typeof(T), ignoreGlobalUnMatchedParameters);
        }
    }
}

