using System.Threading;
using UnityEngine;

namespace Sanctuary.Adapters
{
    public abstract class UnityApplicationLoaderBase : ScriptableObject
    {
        public abstract Awaitable LoadAsync(CancellationToken cancellationToken);

        public virtual Awaitable UnloadAsync() => AwaitableUtility.CompletedTask;
    }
}
