using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

namespace NPCChat.Core.Validation
{
    /// <summary>
    /// Used for game validation, always enabled
    /// rename to 'Require'
    /// </summary>
    public static class Require
    {
        private readonly record struct ExpressionData(string Name, string Value);
        private static string GetCallerName([CallerMemberName] string callerMemberName = "") => callerMemberName;
        private static string Param<T>(
            T value,
            string expression,
             [CallerArgumentExpression(nameof(value))]
            string valueExpression = "")
        {
            if (value.ToString().Equals(expression))
                return $"{valueExpression}({value})";
            if (expression == valueExpression)
                return $"{valueExpression}({value})";
            return $"{valueExpression}({expression}({value}))";
        }

        internal static void AreNotEqual<T>(
            T expected,
            T actual,
            string message = "",
            [CallerArgumentExpression(nameof(expected))]
            string expectedExpression = "",
            [CallerArgumentExpression(nameof(actual))]
            string actualExpression = ""
            )
        {
            InternalAssert(
                !EqualityComparer<T>.Default.Equals(expected, actual),
                message,
                () => [Param(expected, expectedExpression), Param(actual, actualExpression)]);
        }

        internal static void IsGreaterThan<T>(
            T lowerBound,
            T value,
            string message = "",
            [CallerArgumentExpression(nameof(lowerBound))]
            string lowerBoundExpression = "",
            [CallerArgumentExpression(nameof(value))]
            string valueExpression = ""
            )
        {
            InternalAssert(
                Comparer<T>.Default.Compare(lowerBound, value) < 0,
                message,
                () => [Param(lowerBound, lowerBoundExpression), Param(value, valueExpression)]);
        }

        private static void InternalAssert(
            bool success,
            string message,
            Func<IEnumerable<string>> expressions,
            [CallerMemberName] string callerMemberName = "")
        {
            if (success)
                return;
            if (!string.IsNullOrEmpty(message))
                throw new NotImplementedException();
            var errorMessage = $"{callerMemberName}({string.Join(',', expressions())})";
            throw new Exception(errorMessage);

        }
    }

}
