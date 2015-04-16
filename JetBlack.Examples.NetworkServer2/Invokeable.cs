using System;

namespace JetBlack.Examples.NetworkServer2
{
    public static class Invokeable
    {
        public static Action Chain(params Action[] actions)
        {
            return () => actions.ForEach(action => action());
        }

        public static Action<T> Chain<T>(params Action<T>[] actions)
        {
            return arg => actions.ForEach(action => action(arg));
        }

        public static Action<T1, T2> Chain<T1, T2>(params Action<T1, T2>[] actions)
        {
            return (arg1, arg2) => actions.ForEach(action => action(arg1, arg2));
        }

    }
}