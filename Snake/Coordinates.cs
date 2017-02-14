using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    class Coordinates<T>
    {
        private T[,] items;

        public Coordinates()
        {
            items = new T[0, 0];
        }

        public Coordinates(int capacityLeft,int capacityTop)
        {
            if (capacityLeft <= 0 || capacityTop <= 0)
                throw new ArgumentOutOfRangeException("capacity","The capacity has to be greater than 0");

            items = new T[capacityLeft, capacityTop];
        }

        public int GetCapacity(int dimension)
        {
            return items.GetLength(dimension);
        }
        public void SetCapacity(int valueLeft, int valueTop)
        {
            if (valueLeft < items.GetLength(0) || valueTop < items.GetLength(1))
            {
                throw new ArgumentOutOfRangeException("value", "The new capacity must be at least as big as the current");
            }

            if (valueLeft != items.GetLength(0) && valueTop != items.GetLength(1))
            {
                T[,] newItems = new T[valueLeft, valueTop];
                Array.Copy(items, 0, newItems, 0, items.Length);
                items = newItems;
            }
        }

        public T this[int index1, int index2]
        {
            get { return items[index1, index2]; }
            set
            {
                items[index1, index2] = value;
            }
        }

        public void Clear()
        {
            int capacityLeft = items.GetLength(0);
            int capacityTop = items.GetLength(1);

            items = new T[capacityLeft, capacityTop];
        }

        public void Add(T item, int dimension)
        {
            if (dimension == 0)
            {
                SetCapacity(items.GetLength(0) + 1, items.GetLength(1));
            }
            else
            {
                SetCapacity(items.GetLength(0), items.GetLength(1) + 1);
            }

            items[items.GetLength(0)-1, items.GetLength(1)-1] = item;
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int i = 0; i < items.GetLength(0); i++)
                    for (int j = 0; j < items.GetLength(1); j++)
                        if (items[i,j] == null)
                            return true;
                return false;
            }
            else
            {
                EqualityComparer<T> c = EqualityComparer<T>.Default;
                for (int i = 0; i < items.GetLength(0); i++)
                    for (int j = 0; j < items.GetLength(1); j++)
                        if (c.Equals(items[i,j], item))
                            return true;
                return false;
            }
        }

        public void CopyTo(T[] array)
        {
            CopyTo(0, array, 0, items.Length);
        }
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            T[,] tmpItems = items.Clone() as T[,];
            Array.Copy(tmpItems, index, array, arrayIndex, count);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(0, array, arrayIndex, items.Length);
        }

        public bool Exists(T searchTerm)
        {
            return FindIndex(searchTerm) != new Point(-1,-1);
        }

        public T Find(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("match", "Match cannot be null");

            for (int i = 0; i < items.GetLength(0); i++)
                for (int j = 0; j < items.GetLength(1); j++)
                    if (match(items[i, j]))
                        return items[i, j];

            return default(T);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("match", "Match cannot be null");

            List<T> list = new List<T>();
            for (int i = 0; i < items.GetLength(0); i++)
                for (int j = 0; j < items.GetLength(1); j++)
                    if (match(items[i, j]))
                        list.Add(items[i, j]);

            return list;
        }

        public T FindLast(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("match", "Match cannot be null");

            for (int i = items.GetLength(0) - 1; i >= 0; i--)
                for (int j = items.GetLength(1) - 1; j >= 0; j--)
                    if (match(items[i, j]))
                        return items[i, j];

            return default(T);
        }

        public Point FindIndex(Predicate<T> match)
        {
            return FindIndex(0, 0, items.GetLength(0)-1, items.GetLength(1)-1, match);
        }
        public Point FindIndex(int startIndexLeft, int startIndexTop, Predicate<T> match)
        {
            return FindIndex(startIndexLeft, startIndexTop, items.GetLength(0)-1 - startIndexLeft, items.GetLength(1)-1 - startIndexTop, match);
        }
        public Point FindIndex(int startIndexLeft, int startIndexTop, int countLeft, int countTop, Predicate<T> match)
        {
            if (countLeft < 0 || countTop < 0)
                throw new ArgumentOutOfRangeException("countLeft, countTop", "The count must be bigger than 0");

            if (startIndexLeft > items.GetLength(0)-1 - countLeft || startIndexTop > items.GetLength(1)-1 - countTop)
                throw new ArgumentOutOfRangeException();

            if (match == null)
                throw new ArgumentNullException("match", "Match cannot be null");

            int[] endIndex = { startIndexLeft + countLeft, startIndexTop + countTop };
            for (int i = startIndexLeft; i < endIndex[0]; i++)
                for (int j = startIndexTop; j < endIndex[1]; j++)
                    if (match(items[i, j]))
                        return new Point(i, j);

            return new Point(-1, -1);
        }
        public Point FindIndex(T searchTerm)
        {
            return FindIndex(0, 0, items.GetLength(0)-1, items.GetLength(1)-1, x => EqualityComparer<T>.Default.Equals(x, searchTerm));
        }

        public Point FindIndex(int startIndexLeft, int startIndexTop, T searchTerm)
        {
            return FindIndex(startIndexLeft,startIndexTop, items.GetLength(0)-1 - startIndexLeft, items.GetLength(1)-1 - startIndexTop, x => EqualityComparer<T>.Default.Equals(x, searchTerm));
        }

        public Point FindIndex(int startIndexLeft, int startIndexTop, int countLeft, int countTop, T searchTerm)
        {
            return FindIndex(startIndexLeft, startIndexTop, countLeft, countTop, x => EqualityComparer<T>.Default.Equals(x, searchTerm));
        }

        public List<Point> FindAllIndexes(Predicate<T> match)
        {
            return FindAllIndexes(0, 0, items.GetLength(0)-1, items.GetLength(1)-1, match);
        }
        public List<Point> FindAllIndexes(int startIndexLeft, int startIndexTop, Predicate<T> match)
        {
            return FindAllIndexes(startIndexLeft, startIndexTop, items.GetLength(0)-1 - startIndexLeft, items.GetLength(1)-1 - startIndexTop, match);
        }
        public List<Point> FindAllIndexes(int startIndexLeft, int startIndexTop, int countLeft, int countTop, Predicate<T> match)
        {
            if (countLeft < 0 || countTop < 0)
                throw new ArgumentOutOfRangeException("countLeft, countTop", "The count must be bigger than 0");

            if (startIndexLeft > items.GetLength(0)-1 - countLeft || startIndexTop > items.GetLength(1)-1 - countTop)
                throw new ArgumentOutOfRangeException("countLeft, countTop, startIndexLeft, startIndexTop");

            if (match == null)
                throw new ArgumentNullException("match", "Match cannot be null");

            List<Point> l = new List<Point>();
            int[] endIndex = { startIndexLeft + countLeft, startIndexTop + countTop };
            for (int i = startIndexLeft; i < endIndex[0]; i++)
                for (int j = startIndexTop; j < endIndex[1]; j++)
                    if (match(items[i, j]))
                        l.Add(new Point(i, j));

            return l;
        }
        public List<Point> FindAllIndexes(T searchTerm)
        {
            return FindAllIndexes(0, 0, items.GetLength(0)-1, items.GetLength(1)-1, x => EqualityComparer<T>.Default.Equals(x, searchTerm));
        }

        public List<Point> FindAllIndexes(int startIndexLeft, int startIndexTop, T searchTerm)
        {
            return FindAllIndexes(startIndexLeft, startIndexTop, items.GetLength(0)-1 - startIndexLeft, items.GetLength(1)-1 - startIndexTop, x => EqualityComparer<T>.Default.Equals(x, searchTerm));
        }

        public List<Point> FindAllIndexes(int startIndexLeft, int startIndexTop, int countLeft, int countTop, T searchTerm)
        {
            return FindAllIndexes(startIndexLeft, startIndexTop, countLeft, countTop, x => EqualityComparer<T>.Default.Equals(x, searchTerm));
        }
    }
}