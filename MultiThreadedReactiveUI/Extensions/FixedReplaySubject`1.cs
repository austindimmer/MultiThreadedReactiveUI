using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reflection;
namespace MultiThreadedReactiveUI.Extensions
{
    public class FixedReplaySubject<T> : ISubject<T>
    {
        private ReplaySubject<T> _inner;
        private Func<Queue<TimeInterval<T>>> _snapshotGetter;

        public FixedReplaySubject(ReplaySubject<T> source)
        {
            _inner = source;
            var expr = Expression.Lambda(
                typeof(Func<Queue<TimeInterval<T>>>),
                Expression.Field(
                    Expression.Constant(source),
                    source.GetType()
                        .GetField("_queue", BindingFlags.NonPublic | BindingFlags.Instance)));
            _snapshotGetter = (Func<Queue<TimeInterval<T>>>)expr.Compile();
        }

        public IEnumerable<TimeInterval<T>> Snapshot()
        {
            return _snapshotGetter();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _inner.Subscribe(observer);
        }
        public void OnNext(T value)
        {
            _inner.OnNext(value);
        }
        public void OnCompleted()
        {
            _inner.OnCompleted();
        }
        public void OnError(Exception error)
        {
            _inner.OnError(error);
        }
        public void Dispose()
        {
            _inner.Dispose();
        }
    }
}