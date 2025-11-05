using System;

namespace CoreLogic.Classes
{
    public class CircularLinkedList<T>
    {
        public class Node
        {
            public T Value { get; set; }
            public Node Next { get; set; }

            public Node(T value)
            {
                Value = value;
                Next = this; // При создании узел указывает сам на себя
            }
        }

        private Node _tail; // Указатель на последний элемент (в кольцевом списке tail->Next = head)
        private int _count;

        public CircularLinkedList()
        {
            _tail = null;
            _count = 0;
        }

        public int Count => _count;

        public bool IsEmpty => _count == 0;

        /// <summary>
        /// Добавляет элемент в начало списка
        /// </summary>
        public void AddFirst(T value)
        {
            var newNode = new Node(value);

            if (IsEmpty)
            {
                _tail = newNode;
            }
            else
            {
                newNode.Next = _tail.Next; // Новый узел указывает на текущий первый
                _tail.Next = newNode;      // Последний узел теперь указывает на новый
            }

            _count++;
        }

        /// <summary>
        /// Добавляет элемент в конец списка
        /// </summary>
        public void AddLast(T value)
        {
            AddFirst(value);

            if (_count > 1)
            {
                // После добавления в начало, сдвигаем tail к новому последнему элементу
                _tail = _tail.Next;
            }
        }

        public Node AddLastAndGetNode(T item)
        {
            var newNode = new Node(item);

            if (IsEmpty)
            {
                _tail = newNode;
                // newNode.Next уже указывает сам на себя
            }
            else
            {
                // 1. Новый узел должен указывать туда же, куда указывал старый _tail (т.е. на голову)
                newNode.Next = _tail.Next;
                // 2. Старый _tail теперь указывает на новый узел
                _tail.Next = newNode;
                // 3. Новый узел становится новым _tail
                _tail = newNode;
            }

            _count++;
            return newNode;
        }

        /// <summary>
        /// Удаляет первый элемент списка
        /// </summary>
        public bool RemoveFirst()
        {
            if (IsEmpty) return false;

            if (_count == 1)
            {
                _tail = null;
            }
            else
            {
                _tail.Next = _tail.Next.Next; // Пропускаем первый элемент
            }

            _count--;
            return true;
        }

        /// <summary>
        /// Удаляет последний элемент списка
        /// </summary>
        public bool RemoveLast()
        {
            if (IsEmpty) return false;

            if (_count == 1)
            {
                _tail = null;
                _count--;
                return true;
            }

            // Находим предпоследний элемент
            Node current = _tail.Next;
            while (current.Next != _tail)
            {
                current = current.Next;
            }

            // Обновляем связи
            current.Next = _tail.Next;
            _tail = current;
            _count--;
            return true;
        }

        /// <summary>
        /// Удаляет все элементы с указанным значением
        /// </summary>
        public bool Remove(T item)
        {
            if (IsEmpty) return false;

            Node current = _tail.Next; // Голова списка
            Node prev = _tail;

            // Проходим по всему кольцу
            do
            {
                if (current.Value.Equals(item))
                {
                    // Удаляем current
                    prev.Next = current.Next;

                    // Если удаляемый элемент был единственным
                    if (current == _tail && current.Next == current)
                    {
                        _tail = null;
                    }
                    // Если удаляемый элемент был последним (_tail)
                    else if (current == _tail)
                    {
                        _tail = prev;
                    }
                    // Если удаляемый элемент был первым (головой)
                    else if (current == _tail.Next)
                    {
                        // Голова автоматически сместится, так как prev.Next теперь указывает дальше
                    }

                    _count--;
                    return true;
                }

                prev = current;
                current = current.Next;
            } while (current != _tail.Next); // Пока не вернёмся к голове

            return false;
        }

        /// <summary>
        /// Проверяет наличие элемента в списке
        /// </summary>
        public bool Contains(T value)
        {
            if (IsEmpty) return false;

            Node current = _tail.Next;
            do
            {
                if (current.Value.Equals(value))
                    return true;

                current = current.Next;
            } while (current != _tail.Next);

            return false;
        }

        /// <summary>
        /// Возвращает элемент по индексу
        /// </summary>
        public T GetAt(int index)
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException("Индекс находится вне диапазона списка");

            Node current = _tail.Next;
            for (int i = 0; i < index; i++)
            {
                current = current.Next;
            }

            return current.Value;
        }

        /// <summary>
        /// Очищает список
        /// </summary>
        public void Clear()
        {
            _tail = null;
            _count = 0;
        }

        /// <summary>
        /// Создает итератор для перебора элементов
        /// </summary>
        public Enumerator GetEnumerator() => new Enumerator(this);

        public struct Enumerator
        {
            private readonly CircularLinkedList<T> _list;
            private Node _current;
            private int _position;

            internal Enumerator(CircularLinkedList<T> list)
            {
                _list = list;
                _current = null;
                _position = 0;
            }

            public bool MoveNext()
            {
                if (_position == 0 && !_list.IsEmpty)
                {
                    _current = _list._tail.Next; // Первый узел
                    _position = 1;
                    return true;
                }

                if (_position >= _list._count || _list.IsEmpty)
                    return false;

                _current = _current.Next;
                _position++;
                return _position <= _list._count;
            }

            // Основное свойство - значение
            public T Current => _current.Value;

            // НОВОЕ СВОЙСТВО - ссылка на узел!
            public Node CurrentNode => _current;

            public void Reset()
            {
                _current = null;
                _position = 0;
            }
        }

        /// <summary>
        /// Возвращает строковое представление списка
        /// </summary>
        public override string ToString()
        {
            if (IsEmpty) return "[]";

            var result = new System.Text.StringBuilder();
            result.Append("[");

            Node current = _tail.Next;
            do
            {
                result.Append(current.Value?.ToString() ?? "null");
                current = current.Next;

                if (current != _tail.Next)
                    result.Append(", ");
            } while (current != _tail.Next);

            result.Append("]");
            return result.ToString();
        }
    }
}