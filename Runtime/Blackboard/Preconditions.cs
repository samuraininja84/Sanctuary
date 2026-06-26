using System;
using Sanctuary.Extensions;

namespace Sanctuary.Blackboard
{
    public static class Preconditions
    {
        public static T CheckNotNull<T>(T reference) => CheckNotNull(reference, null);

        public static T CheckNotNull<T>(T reference, string message)
        {
            // If the reference is a UnityEngine.Object and it is null, throw an ArgumentNullException with the provided message
            if (reference is UnityEngine.Object obj && obj.OrNull() == null) throw new ArgumentNullException(message);

            // If the reference is null, throw an ArgumentNullException with the provided message
            if (reference is null) throw new ArgumentNullException(message);

            // If the reference is not null, return it
            return reference;
        }

        public static void CheckState(bool expression) => CheckState(expression, null);

        public static void CheckState(bool expression, string messageTemplate, params object[] messageArgs) => CheckState(expression, string.Format(messageTemplate, messageArgs));

        public static void CheckState(bool expression, string message)
        {
            // If the expression is true, return without doing anything
            if (expression) return;

            // If the expression is false, throw an InvalidOperationException with the provided message
            throw message == null ? new InvalidOperationException() : new InvalidOperationException(message);
        }
    }
}
