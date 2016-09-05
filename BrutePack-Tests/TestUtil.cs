using System;
using NUnit.Framework;

namespace BrutePack_Tests
{
    [Flags]
    public enum TestPathType
    {
        Root = 0,
        MethodName = 1,
        ClassName = 2,
        MethodAndClassName = 3
    }

    public static class TestUtil
    {
        public static string GetTestDataDir(TestPathType pathType = TestPathType.MethodAndClassName)
        {
            var result = TestContext.CurrentContext.TestDirectory + "/../../TestData/";
            if (pathType.HasFlag(TestPathType.ClassName))
            {
                var className = TestContext.CurrentContext.Test.ClassName;
                result += className.Substring(className.LastIndexOf('.') + 1) + "/";
            }
            if (pathType.HasFlag(TestPathType.MethodName))
                result += TestContext.CurrentContext.Test.MethodName + "/";
            return result;
        }
    }
}