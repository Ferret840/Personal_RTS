using System.Collections;
using UnityEngine;
using System;

namespace Tools
{

    public class Heap<T> where T : IHeapItem<T>
    {
        T[] m_Items;
        int m_CurrentItemCount;

        public Heap(int _maxHeapSize)
        {
            m_Items = new T[_maxHeapSize];
        }

        public void Add(T _item)
        {
            _item.HeapIndex = m_CurrentItemCount;
            m_Items[m_CurrentItemCount] = _item;
            SortUp(_item);
            ++m_CurrentItemCount;
        }

        public T RemoveFirst()
        {
            T firstItem = m_Items[0];

            --m_CurrentItemCount;
            m_Items[0] = m_Items[m_CurrentItemCount];
            m_Items[0].HeapIndex = 0;
            SortDown(m_Items[0]);

            return firstItem;
        }

        public void UpdateItem(T _item)
        {
            SortUp(_item);
        }

        public int Count
        {
            get
            {
                return m_CurrentItemCount;
            }
        }

        public bool Contains(T _item)
        {
            return Equals(m_Items[_item.HeapIndex], _item);
        }

        void SortDown(T _item)
        {
            while (true)
            {
                int leftIndex = _item.HeapIndex * 2 + 1;
                int rightIndex = _item.HeapIndex * 2 + 2;
                int swapIndex = 0;

                if (leftIndex < m_CurrentItemCount)
                {
                    swapIndex = leftIndex;

                    if (rightIndex < m_CurrentItemCount)
                    {
                        if (m_Items[leftIndex].CompareTo(m_Items[rightIndex]) < 0)
                        {
                            swapIndex = rightIndex;
                        }
                    }

                    if (_item.CompareTo(m_Items[swapIndex]) < 0)
                    {
                        Swap(_item, m_Items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        void SortUp(T _item)
        {
            int parentIndex = (_item.HeapIndex - 1) / 2;

            while (true)
            {
                T parentItem = m_Items[parentIndex];

                if (_item.CompareTo(parentItem) > 0)
                {
                    Swap(_item, parentItem);
                }
                else
                {
                    break;
                }

                parentIndex = (_item.HeapIndex - 1) / 2;
            }
        }

        void Swap(T _itemA, T _itemB)
        {
            m_Items[_itemA.HeapIndex] = _itemB;
            m_Items[_itemB.HeapIndex] = _itemA;

            int temp = _itemA.HeapIndex;
            _itemA.HeapIndex = _itemB.HeapIndex;
            _itemB.HeapIndex = temp;
        }

        public T[] ToArray()
        {
            T[] array = m_Items;
            return array;
        }
    }

    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex
        {
            get;
            set;
        }
    }

}