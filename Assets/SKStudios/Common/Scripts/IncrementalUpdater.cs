using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eppy;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SKStudios.Common.Utils {
    using MonoActionPair = Tuple<MonoBehaviour, Action>;
    using NodeObjAction = IncrementalUpdater.Node<Tuple<MonoBehaviour, Action>>;
    using MonoInvalidFunc = Func<IncrementalUpdater.Node<Tuple<MonoBehaviour, Action>>, bool>;
    using StepDict = Dictionary<UpdateStep, Dictionary<Type, IncrementalUpdater.Node<Tuple<MonoBehaviour, Action>>>>;
    using NodeDict = Dictionary<Type, IncrementalUpdater.Node<Tuple<MonoBehaviour, Action>>>;

    public enum UpdateStep {
        Update,
        LateUpdate,
        FixedUpdate
    }

    public class ComparisonFuncs {
        public struct UpdateStepComparer : IEqualityComparer<UpdateStep> {
            public bool Equals(UpdateStep x, UpdateStep y) {
                return x == y;
            }

            public int GetHashCode(UpdateStep obj) {
                return (int) obj;
            }
        }

        public struct TypeComparer : IEqualityComparer<Type> {
            public bool Equals(Type x, Type y) {
                return x == y;
            }

            public int GetHashCode(Type obj) {
                return obj.GetHashCode();
            }
        }
    }

    /// <summary>
    ///     Class that procs events on one object per frame for expensive updates that do not need to be done constantly
    /// </summary>
    public class IncrementalUpdater : MonoBehaviour {
        private static readonly StepDict ActionDictionary;

        private static readonly MonoInvalidFunc InvalidFunc = n =>
            n.GetData().Item1 == null || n.GetData().Item2 == null;

        private static bool _loading;
        private static bool _abort;

        private static IncrementalUpdater _instance;

        static IncrementalUpdater() {
            ActionDictionary = new StepDict(new ComparisonFuncs.UpdateStepComparer());
            foreach (UpdateStep step in Enum.GetValues(typeof(UpdateStep)))
                ActionDictionary.Add(step, new Dictionary<Type, NodeObjAction>(new ComparisonFuncs.TypeComparer()));

            SceneManager.sceneLoaded += (s, s2) => {
                Reset();
                _loading = true;
            };

            SceneManager.activeSceneChanged += (s, s2) => { _loading = false; };
        }

        private static IncrementalUpdater Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<IncrementalUpdater>();
                    if (_instance != null)
                        return _instance;

                    var instanceObj = new GameObject("Incremental Updater");
                    _instance = instanceObj.AddComponent<IncrementalUpdater>();
                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }

        /// <summary>
        ///     Register an object and an action to be submitted to the Incremental Updater.
        /// </summary>
        /// <typeparam name="T">MonoBehavior to unregister</typeparam>
        /// <param name="obj">Object to add</param>
        /// <param name="action">Action to add</param>
        public static void RegisterObject<T>(MonoBehaviour obj, Action action, UpdateStep step)
            where T : MonoBehaviour {
            var touch = Instance;
            Func<bool> register = () => {
                Dictionary<Type, NodeObjAction> actionDict;
                lock (ActionDictionary) {
                    if (!ActionDictionary.TryGetValue(step, out actionDict))
                        return false;
                }

                lock (actionDict) {
                    var key = typeof(T);

                    var node = new NodeObjAction(new MonoActionPair(obj, action), InvalidFunc);
                    NodeObjAction curNode;

                    if (!actionDict.TryGetValue(key, out curNode))
                        actionDict.Add(key, node);

#if SKS_DEV
                    Debug.Log(string.Format("Registered {0}", node.GetData()));
#endif
                    return true;
                }
            };
            if (!register())
                new Thread(() => {
                    while (!register() && !_abort) Thread.Sleep(100);
                }).Start();
        }

        /// <summary>
        ///     Unregisters an object from a given step
        /// </summary>
        /// <typeparam name="T">MonoBehavior to unregister</typeparam>
        /// <param name="obj">Object to remove</param>
        /// <param name="step">Step to remove from</param>
        public static void RemoveObject<T>(MonoBehaviour obj, UpdateStep step) {
            Dictionary<Type, NodeObjAction> actionDict;
            if (!ActionDictionary.TryGetValue(step, out actionDict)) return;
            NodeObjAction value;
            if (!actionDict.TryGetValue(typeof(T), out value)) return;
            if (value == null) return;

#if SKS_DEV
            Debug.Log(string.Format("Unregistered {0}", value.GetData()));
#endif
            value.Destroy();
        }

        private static void Reset() {
            foreach (var step in ActionDictionary.Keys)
                ActionDictionary[step].Clear();
        }

        private void Update() {
            ExecuteStep(UpdateStep.Update);
        }

        private void FixedUpdate() {
            ExecuteStep(UpdateStep.LateUpdate);
        }

        private void LateUpdate() {
            ExecuteStep(UpdateStep.FixedUpdate);
        }

        private void OnDestroy() {
            _abort = true;
        }

        private void ExecuteStep(UpdateStep step) {
            Dictionary<Type, NodeObjAction> actionDict;
            if (!ActionDictionary.TryGetValue(step, out actionDict)) return;

            var types = actionDict.Keys.ToArray();
            foreach (var key in types) {
                var value = actionDict[key];
                NodeObjAction next;
                value.GetData(out next).Item2();
                actionDict[key] = next;
            }

            /*using (var enumerator = actionDict.GetEnumerator())
            {
                while (enumerator.MoveNext()) {
                    var element = enumerator.Current;
                    var key = element.Key;
                    var value = actionDict[key];
                    NodeObjAction next;
                    value.GetData(out next).Item2();
                    actionDict[key] = next;
                }
            }*/
        }

        internal sealed class Node<T> : IEquatable<Node<T>> where T : class {
            private static Node<T> _lastPlaced;
            private readonly T _data;
            private readonly Func<Node<T>, bool> _invalidFunc;
            private Node<T> _nextNode;
            private Node<T> _prevNode;

            /// <summary>
            ///     Returns a new node
            /// </summary>
            /// <param name="data">Data within this node</param>
            /// <param name="invalidFunc">The function that, when true, marks this node as invalid</param>
            public Node(T data, Func<Node<T>, bool> invalidFunc) {
                _invalidFunc = invalidFunc;
                _data = data;
                if (_lastPlaced == null)
                    _lastPlaced = this;
                NextNode = _lastPlaced.NextNode;
                _lastPlaced.NextNode = this;
            }

            private Node<T> NextNode {
                get {
                    if (_nextNode == null) {
                        _nextNode = this;
                        goto RETURN;
                    }

                    if (_invalidFunc(_nextNode)) {
                        _nextNode = _nextNode._nextNode;
                        while (!_nextNode.Equals(this))
                            if (_invalidFunc(_nextNode)) {
                                _nextNode = _nextNode._nextNode;
#if SKS_DEV
                                Debug.Log(string.Format("Unregistered {0}", _nextNode._data));
#endif
                            }
                            else {
                                goto RETURN;
                            }

                        if (_nextNode != null)
                            _nextNode._prevNode = this;
                    }

                    RETURN:
                    return _nextNode;
                }
                set { _nextNode = value; }
            }

            public bool Equals(Node<T> other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(_invalidFunc, other._invalidFunc) &&
                       EqualityComparer<T>.Default.Equals(_data, other._data);
            }

            /// <summary>
            ///     Destroy this node
            /// </summary>
            public void Destroy() {
                var next = _nextNode;
                if (_prevNode != null && next != null) {
                    //NextNode
                    _prevNode._nextNode = next;
                    next._prevNode = _prevNode;
                }
                else {
                    _lastPlaced = null;
                }
            }

            /// <summary>
            ///     Returns the data, as well as the next node
            /// </summary>
            /// <returns>Data stored in this node</returns>
            public T GetData(out Node<T> next) {
                next = NextNode;
                return GetData();
            }

            public T GetData() {
                return _data;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Node<T>) obj);
            }

            public override int GetHashCode() {
                unchecked {
                    var hashCode = _invalidFunc != null ? _invalidFunc.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(_data);
                    hashCode = (hashCode * 397) ^ (_prevNode != null ? _prevNode.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (_nextNode != null ? _nextNode.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
    }
}