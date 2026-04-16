using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using NPCChat.Core.Extensions;

namespace NPCChat.Core.Validation
{
    /// <summary>
    /// Used for game validation, always enabled
    /// rename to 'Require'
    /// </summary>
    public static class Require
    {
        public static void IsNull<T>(
            T value,
            string message = "",
            [CallerArgumentExpression(nameof(value))]
            string valueExpression = ""
            )
        {
            InternalAssert(
                value is null,
                message,
                () => new string[] { Param(value, valueExpression) });
        }

        public static void IsNotNull<T>(
            T value,
            string message = "",
            [CallerArgumentExpression(nameof(value))]
                    string valueExpression = ""
            )
        {
            InternalAssert(
                value is not null,
                message,
                () => new string[] { Param(value, valueExpression) });
        }

        public static void AreNotEqual<T>(
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
                () => new string[] { Param(expected, expectedExpression), Param(actual, actualExpression) });
        }

        public static void IsGreaterThan<T>(
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
                () => new string[] { Param(lowerBound, lowerBoundExpression), Param(value, valueExpression) });
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

        private static string Param<T>(
            T value,
            string expression,
             [CallerArgumentExpression(nameof(value))]
            string valueExpression = "")
        {
            var valueStr = value.AsString();
            if (valueStr.Equals(expression))
                return $"{valueExpression}({valueStr})";
            if (expression == valueExpression)
                return $"{valueExpression}({valueStr})";
            return $"{valueExpression}({expression}({valueStr}))";
        }
    }
}
