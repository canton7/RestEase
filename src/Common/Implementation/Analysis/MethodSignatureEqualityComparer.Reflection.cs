using System;

namespace RestEase.Implementation.Analysis
{
    internal partial class MethodSignatureEqualityComparer
    {
        public bool Equals(MethodModel x, MethodModel y)
        {
            var xInfo = x?.MethodInfo;
            var yInfo = y?.MethodInfo;
            if (xInfo == yInfo)
                return true;
            if (xInfo == null || yInfo == null)
                return false;

            if (xInfo.Name != yInfo.Name)
                return false;
            var xParameters = xInfo.GetParameters();
            var yParameters = yInfo.GetParameters();
            if (xParameters.Length != yParameters.Length)
                return false;
            if (xInfo.IsGenericMethod != yInfo.IsGenericMethod)
                return false;
            var xGenericArgs = xInfo.GetGenericArguments();
            var yGenericArgs = yInfo.GetGenericArguments();
            if (xInfo.IsGenericMethod && xGenericArgs.Length != yGenericArgs.Length)
                return false;
            for (int i = 0; i < xParameters.Length; i++)
            {
                var xParam = xParameters[i];
                var yParam = yParameters[i];
                if (xParam.ParameterType.IsGenericParameter != yParam.ParameterType.IsGenericParameter)
                {
                    return false;
                }
                if (xParam.ParameterType.IsByRef != yParam.ParameterType.IsByRef)
                {
                    return false;
                }

                if (xParam.ParameterType.IsGenericParameter)
                {
                    if (Array.IndexOf(xGenericArgs, xParam.ParameterType) != Array.IndexOf(yGenericArgs, yParam.ParameterType))
                        return false;
                }
                else if (xParam.ParameterType != yParam.ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(MethodModel model)
        {
            var obj = model?.MethodInfo;
            if (obj == null)
                return 0;

            // We don't need everything, just be sensible
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + obj.Name.GetHashCode();
                hash = hash * 23 + obj.GetParameters().Length;
                return hash;
            }
        }
    }
}
