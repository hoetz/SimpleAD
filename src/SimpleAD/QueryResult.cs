using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace SimpleAD
{
    public class QueryResult : DynamicObject, IEnumerable
    {
        private readonly IEnumerable<IEnumerable<dynamic>> _sources;
        private readonly IEnumerator<IEnumerable<dynamic>> _sourceEnumerator;
        private bool _hasCurrent;

        public IEnumerator GetEnumerator()
        {
            return new DynamicEnumerator(_sourceEnumerator.Current);
        }

        private QueryResult()
            : this(Enumerable.Empty<IEnumerable<dynamic>>())
        {
        }

        public QueryResult(params IEnumerable<dynamic>[] sources)
            : this(sources.AsEnumerable())
        {
        }

        public QueryResult(IEnumerable<IEnumerable<dynamic>> sources)
        {
            _sources = sources;
            _sourceEnumerator = _sources.GetEnumerator();
            _hasCurrent = _sourceEnumerator.MoveNext();
        }

        public bool NextResult()
        {
            return _hasCurrent = (_hasCurrent && _sourceEnumerator.MoveNext());
        }

        public dynamic First()
        {
            return _sourceEnumerator.Current.First();
        }

        public dynamic FirstOrDefault()
        {
            return _sourceEnumerator.Current.FirstOrDefault();
        }

        public IList<dynamic> ToList()
        {
            return _sourceEnumerator.Current.ToList();
        }

        public int Count()
        {
            return _sourceEnumerator.Current.Count();
        }

        public dynamic[] ToArray()
        {
            return _sourceEnumerator.Current.ToArray();
        }

        public dynamic Last()
        {
            return _sourceEnumerator.Current.Last();
        }

        public dynamic LastOrDefault()
        {
            return _sourceEnumerator.Current.LastOrDefault();
        }
    }

    internal class DynamicEnumerator : IEnumerator, IDisposable
    {
        //contains code taken from https://raw.githubusercontent.com/markrendle/Simple.Data/65ff01e482ce71190869001c221caa140feb70e5/Simple.Data/SimpleResultSet.cs

        private readonly IEnumerator<dynamic> _enumerator;

        public DynamicEnumerator(IEnumerable<dynamic> source)
        {
            _enumerator = source.GetEnumerator();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _enumerator.Dispose();
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public void Reset()
        {
            _enumerator.Reset();
        }

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        /// <returns>
        /// The current element in the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception><filterpriority>2</filterpriority>
        public object Current
        {
            get { return _enumerator.Current; }
        }
    }
}