namespace STROOP.Core
{
#pragma warning disable CA1063 // Implement IDisposable correctly - this is not a resource!
    public abstract class Scope : IDisposable
    {
        protected Scope()
        {
        }

        protected abstract void Close();
        void IDisposable.Dispose() => Close();
    }
#pragma warning restore CA1063

    public sealed class AccessScope<T> : Scope
    {
        public static T content => scopes.Count == 0 ? default(T) : scopes.Peek().obj;

        static Stack<AccessScope<T>> scopes = new Stack<AccessScope<T>>();
        T obj;

        public AccessScope(T obj)
        {
            this.obj = obj;
            scopes.Push(this);
        }

        protected override void Close()
        {
            if (scopes.Pop() != this)
                throw new Exception($"Scopes must be disposed in reverse creation order.");
        }
    }

    public class IgnoreScope : Scope
    {
        Stack<IgnoreScope> scopeStack = new Stack<IgnoreScope>();

        IgnoreScope parent;

        public IgnoreScope()
        {
        }

        private IgnoreScope(IgnoreScope parent)
        {
            this.parent = parent;
        }

        public IgnoreScope New()
        {
            var newScope = new IgnoreScope(this);
            scopeStack.Push(newScope);
            return newScope;
        }

        protected override void Close()
        {
            if (parent.scopeStack.Pop() != this)
                throw new InvalidOperationException("IgnoreScopes popped in invalid order");
        }

        public bool ignore => scopeStack.Count > 0;

        public static implicit operator bool(IgnoreScope ignoreScope) => ignoreScope.ignore;
    }
}
