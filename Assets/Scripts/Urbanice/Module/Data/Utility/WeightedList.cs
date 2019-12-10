using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Urbanice.Module.Data.Utility
{
    /// <summary>
    /// Generic WeightedList to manage weighted elements
    /// </summary>
    public class WeightedList<U, T> where U : WeightedElement<T> where T : Enum
    {
        private int _overallWeight;
        private readonly List<U> _elements;

        public int OverallWeight => _overallWeight;

        public WeightedList()
        {
            _overallWeight = 0;
            _elements = new List<U>();
        }

        public void Add(U element)
        {
            if(element.Weight <= 0)
                return;

            _elements.Add(element);
            _elements.Sort((w1, w2) => w1.Weight > w2.Weight ? -1 : 1);
            _overallWeight += element.Weight;
        }

        public void RemoveAll(T type)
        {
            for (var i =_elements.Count-1; i >= 0; i--)
            {
                if (_elements[i].Element.Equals(type))
                {
                    RemoveAt(i);
                }
            }
        }

        private void RemoveAt(int index)
        {
            _overallWeight -= _elements[index].Weight;
            _elements.RemoveAt(index);
        }

        public T GetElement(double value)
        {
            if (_elements.Count == 0)
            {
                throw new Exception("There are no elements in the list");
            }
            if (value < 0)
            {
                value = 0;
            }
            else if(value > 1)
            {
                value = 1 - 1e-10;
            }

            int requestedWeight = (int)(value*_overallWeight);
            int sum = 0;
            int cnt = 0;
            foreach (var e in _elements)
            {
                sum += e.Weight;
                if (sum >= requestedWeight)
                {
                    break;
                }

                cnt++;

            }
            var element = _elements[cnt];
            _elements.Remove(element);
            _overallWeight -= element.Weight;
            
            return element.Element;
        }
    }
}