using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MultiThreadedReactiveUI.Extensions
{
    public static class RxExtensions
    {

        public static IObservable<IList<T>> BufferUntilInactive<T>(this IObservable<T> stream, TimeSpan delay)
        {
            var closes = stream.Throttle(delay);
            return stream.Window(() => closes).SelectMany(window => window.ToList());
        }

        public static IObservable<IList<T>> BufferUntilInactive<T>(this IObservable<T> stream, TimeSpan delay, Int32? max = null)
        {
            var closes = stream.Throttle(delay);
            if (max != null)
            {
                var overflows = stream.Where((x, index) => index + 1 >= max);
                closes = closes.Merge(overflows);
            }
            return stream.Window(() => closes).SelectMany(window => window.ToList());
        }

        public static IObservable<IEnumerable<T>> BufferWithInactivity<T>(
    this IObservable<T> source,
    TimeSpan inactivity,
    int maximumBufferSize)
        {
            return Observable.Create<IEnumerable<T>>(o =>
            {
                var gate = new object();
                var buffer = new List<T>();
                var mutable = new SerialDisposable();
                var subscription = (IDisposable)null;
                var scheduler = Scheduler.ThreadPool;

                Action dump = () =>
                {
                    var bts = buffer.ToArray();
                    buffer = new List<T>();
                    if (o != null)
                        o.OnNext(bts);
                };

                Action dispose = () =>
                {
                    if (subscription != null)
                        subscription.Dispose();
                    mutable.Dispose();
                };

                Action<Action<IObserver<IEnumerable<T>>>> onErrorOrCompleted =
                    onAction =>
                    {
                        lock (gate)
                        {
                            dispose();
                            dump();
                            if (o != null)
                                onAction(o);
                        }
                    };

                Action<Exception> onError = ex =>
                    onErrorOrCompleted(x => x.OnError(ex));

                Action onCompleted = () => onErrorOrCompleted(x => x.OnCompleted());

                Action<T> onNext = t =>
                {
                    lock (gate)
                    {
                        buffer.Add(t);
                        if (buffer.Count == maximumBufferSize)
                        {
                            dump();
                            mutable.Disposable = Disposable.Empty;
                        }
                        else
                            mutable.Disposable = scheduler.Schedule(inactivity, () =>
                            {
                                lock (gate)
                                    dump();
                            });
                    }
                };

                subscription =
                    source
                        .ObserveOn(scheduler)
                        .Subscribe(onNext, onError, onCompleted);

                return () =>
                {
                    lock (gate)
                    {
                        o = null;
                        dispose();
                    }
                };
            });
        }


        public static void Dump<T>(this IObservable<T> source, string name)
        {
            source.Subscribe(
            i => Console.WriteLine("{0}-->{1}", name, i),
            ex => Console.WriteLine("{0} failed-->{1}", name, ex.Message),
            () => Console.WriteLine("{0} completed", name));
        }
        public static IObservable<T[]> RollingBuffer<T>(
            this IObservable<T> @this,
            TimeSpan buffering)
        {
            return Observable.Create<T[]>(o =>
            {
                var list = new LinkedList<Timestamped<T>>();
                return @this.Timestamp().Subscribe(tx =>
                {
                    list.AddLast(tx);
                    while (list.First.Value.Timestamp < DateTime.Now.Subtract(buffering))
                        list.RemoveFirst();
                    o.OnNext(list.Select(tx2 => tx2.Value).ToArray());
                }, ex => o.OnError(ex), () => o.OnCompleted());
            });
        }

        public static IObservable<IList<T>> SlidingWindow<T>(
            this IObservable<T> src,
            int windowSize)
        {
            //Call this method as follows
            //var source = Observable.Range(0, 10);
            //var query = source.SlidingWindow(3);
            //using (query.Subscribe(Console.WriteLine))
            //{
            //    Console.ReadLine();
            //}
            var feed = src.Publish().RefCount();
            // (skip 0) + (skip 1) + (skip 2) + ... + (skip nth) => return as list  
            return Observable.Zip(
            Enumerable.Range(0, windowSize)
                .Select(skip =>
                {
                    Console.WriteLine("Skipping {0} els", skip);
                    return feed.Skip(skip);
                })
                .ToArray());
        }

        public static IObservable<IReadOnlyList<Timestamped<T>>> SlidingWindow<T>(this IObservable<Timestamped<T>> self, TimeSpan length)
        {
            return self.Scan(new LinkedList<Timestamped<T>>(),
                             (ll, newSample) =>
                             {
                                 ll.AddLast(newSample);
                                 var oldest = newSample.Timestamp - length;
                                 while (ll.Count > 0 && ll.First.Value.Timestamp < oldest)
                                     ll.RemoveFirst();
                                 return ll;
                             }).Select(l => l.ToList().AsReadOnly());
        }

        public static List<T> Snapshot<T>(ReplaySubject<T> subject)
        {
            List<T> snapshot = new List<T>();
            using (subject.Subscribe(item => snapshot.Add(item)))
            {
                // Deliberately empty; subscribing will add everything to the list.
                foreach (var t in snapshot)
                {

                }
            }
            return snapshot;
        }


    }
}
