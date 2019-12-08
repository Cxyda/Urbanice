using System;
using System.Collections.Generic;

namespace Urbanice.Data
{
    public class Graph<T>
    {
        public List<T> Nodes => new List<T>(_nodes);
        
        private readonly List<T> _nodes;
        private readonly List<List<int>> _connections;
        
        public Graph()
        {
            _nodes = new List<T>();
            _connections = new List<List<int>>();
        }
        public void AddNode(T node)
        {
            if (_nodes.Contains(node))
            {
                return;
            }
            _nodes.Add(node);
            _connections.Add(new List<int>());
        }
        public void AddNode(T node, params int[] connections)
        {
            if (_nodes.Contains(node))
            {
                return;
            }
            
            _nodes.Add(node);
            _connections.Add(new List<int>(connections));
        }

        public bool ConnectNodes(T node1, T node2, bool directional = false)
        {
            var index1 = _nodes.IndexOf(node1);
            var index2 = _nodes.IndexOf(node2);

            if (index1 < 0 || index2 < 0)
            {
                return false;
            }

            if (!_connections[index1].Contains(index2))
            {
                _connections[index1].Add(index2);
            }

            if (!directional && !_connections[index2].Contains(index1))
            {
                _connections[index2].Add(index1);
            }
            return true;
        }

        /// <summary>
        /// Fuses two nodes into the first one and removes the second one
        /// </summary>
        public bool FuseNodes(T node1, T node2)
        {
            var index1 = _nodes.IndexOf(node1);
            var index2 = _nodes.IndexOf(node2);
            
            if (index1 < 0 || index2 < 0)
            {
                throw new Exception("At least one node is not part of the graph");
            }

            if (index1 == index2)
            {
                return false;
            }

            var connections = new List<int>(_connections[index2]);
            foreach (var cid in (connections))
            {
                if (cid != index1 && !_connections[index1].Contains(cid))
                {
                    _connections[index1].Add(cid);
                }
            }

            // reconnect connections at the other nodes
            for (int i = 0; i < _connections.Count; i++)
            {
                if (i == index2) continue;
                if (i == index1 && _connections[i].Contains(index2))
                {
                    _connections[i].Remove(index2);
                    continue;
                }

                var index = _connections[i].IndexOf(index2);
                if (index >= 0)
                {
                    _connections[i][index] = index1;
                }
            }
            _nodes.RemoveAt(index2);
            _connections.RemoveAt(index2);

            return true;
        }

        public bool DisconnectNodes(T node1, T node2, bool directional = false)
        {
            var index1 = _nodes.IndexOf(node1);
            var index2 = _nodes.IndexOf(node2);

            if (index1 < 0 || index2 < 0)
            {
                return false;
            }

            _connections[index1].Remove(index2);
            _connections[index2].Remove(index1);

            return true;
        }

        public List<T> GetNeighbors(T node)
        {
            var index = _nodes.IndexOf(node);
            if (index < 0)
            {
                return null;
            }
            return GetNeighbors(index);
        }
        public List<T> GetNeighbors(int nodeIndex)
        {
            var neighbors = new List<T>();
            if (nodeIndex < 0) return neighbors;

            foreach (var nodeId in _connections[nodeIndex])
            {
                neighbors.Add(_nodes[nodeId]);
            }

            return neighbors;
        }

        public T GetNode(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= _nodes.Count)
            {
                throw new Exception("Nodeindex is invalid");
            }

            return _nodes[nodeIndex];
        }
        public bool RemoveNode(T node)
        {
            var index = _nodes.IndexOf(node);
            if (index < 0) return false;
            
            _nodes.RemoveAt(index);
            _connections.RemoveAt(index);
            return true;
        }
        public void Clear(T node)
        {
            _nodes.Clear();
            _connections.Clear();
        }
    }
}