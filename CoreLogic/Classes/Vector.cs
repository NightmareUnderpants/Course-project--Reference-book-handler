using System;

namespace CoreLogic.Vector
{
    /// <summary>
    /// Самописный аналог динамического массива, аналогичный <see cref="List{T}"/>
    /// </summary>
    /// <typeparam name="T">
    /// Тип элементов вектора. Может быть <b>любой структурой</b> (<see cref="int"/>, <see cref="float"/>, пользовательские <c>struct</c>)
    /// или <b>классом</b> (но с учётом особенностей работы GC).
    /// </typeparam>
    public class Vector<T>
    {
        private T[] _values;
        private int _count;

        /// <summary>
        /// Конструктор, принимающий как параметр объем свободных ячеек.
        /// </summary>
        /// <param name="capacity">Изначальный объем свободного места.</param>
        public Vector(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative.");

            _values = new T[capacity];
            _count = 0;
        }

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Vector()
        {
            _values = new T[1];
            _count = 0;
        }

        /// <summary>
        /// Количество фактически хранимых элементов.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Текущая ёмкость внутреннего массива (сколько элементов можно вместить без расширения).
        /// </summary>
        public int Capacity => _values.Length;

        /// <summary>
        /// Добавляет элемент в конец вектора. При необходимости увеличивает ёмкость вдвое.
        /// </summary>
        /// <param name="item">Элемент, который нужно добавить.</param>
        public void Add(T item)
        {
            if (_count == _values.Length)
            {
                int newCapacity = (_values.Length == 0) ? 1 : _values.Length * 2;
                Array.Resize(ref _values, newCapacity);
            }
            _values[_count++] = item;
        }

        /// <summary>
        /// Удаляет все элементы из вектора (использует только обнуление счётчика; 
        /// ссылки будут собраны GC по необходимости).
        /// </summary>
        public void Clear()
        {
            //for(int i = 0; i < _count; i++) _values[i] = default;
            _count = 0;
        }

        /// <summary>
        /// Проверяет, содержится ли указанный элемент в векторе; реализация без LINQ.
        /// </summary>
        /// <param name="item">Искомый элемент.</param>
        /// <returns>True, если элемент найден, иначе false.</returns>
        public bool Contains(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (item.Equals(_values[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Копирует элементы вектора в указанный массив, начиная с заданного индекса, без использования Array.Copy.
        /// </summary>
        /// <param name="array">
        /// Одномерный массив, в который копируются элементы. 
        /// Массив должен иметь достаточную ёмкость для размещения всех элементов.
        /// </param>
        /// <param name="arrayIndex">
        /// Отсчитываемый от нуля индекс в массиве <paramref name="array"/>, 
        /// с которого начинается копирование.
        /// </param>
        /// <exception cref="ArgumentNullException">Если <paramref name="array"/> == null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Если <paramref name="arrayIndex"/> меньше 0 или в массиве не хватает места для всех элементов.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < _count)
                throw new ArgumentException("Destination array is not large enough.");

            for (int i = 0; i < _count; i++)
            {
                array[arrayIndex + i] = _values[i];
            }
        }

        /// <summary>
        /// Возвращает индекс первого вхождения указанного элемента в векторе; 
        /// если не найден, возвращает -1. Реализовано вручную.
        /// </summary>
        /// <param name="item">Искомый элемент.</param>
        /// <returns>Индекс элемента или -1, если не найден.</returns>
        public int IndexOf(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (item.Equals(_values[i]))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Вставляет элемент в вектор по указанному индексу. При необходимости расширяет ёмкость.
        /// </summary>
        /// <param name="index">
        /// Отсчитываемый от нуля индекс, по которому должен быть вставлен элемент.
        /// Допустимые значения: от 0 до текущего количества элементов (включительно).
        /// </param>
        /// <param name="item">Вставляемый элемент.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Если <paramref name="index"/> меньше 0 или больше текущего количества элементов.
        /// </exception>
        public void Insert(int index, T item)
        {
            if (index < 0 || index > _count)
                throw new ArgumentOutOfRangeException(nameof(index));

            // Расширяем массив, если он заполнен
            if (_count == _values.Length)
            {
                int newCapacity = (_values.Length == 0) ? 1 : _values.Length * 2;
                Array.Resize(ref _values, newCapacity);
            }

            // Сдвигаем все элементы от index до конца вправо на одну позицию
            for (int i = _count; i > index; i--)
            {
                _values[i] = _values[i - 1];
            }

            _values[index] = item;
            _count++;
        }

        /// <summary>
        /// Удаляет первое вхождение указанного элемента из вектора.
        /// </summary>
        /// <param name="item">Элемент, который требуется удалить.</param>
        /// <returns>True, если элемент был найден и удален; иначе false.</returns>
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;

            // Сдвигаем все элементы справа от index влево на одну позицию
            for (int i = index; i < _count - 1; i++)
            {
                _values[i] = _values[i + 1];
            }

            _count--;
            _values[_count] = default; // Обнуляем «связку» последнего элемента для GC (если ссылочный тип)

            return true;
        }

        /// <summary>
        /// Индексатор для доступа к элементам вектора по индексу.
        /// </summary>
        /// <param name="index">Индекс элемента (0-based).</param>
        /// <returns>Элемент вектора по указанному индексу.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Если <paramref name="index"/> меньше 0 или >= Count.
        /// </exception>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _values[index];
            }
            set
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                _values[index] = value;
            }
        }
    }
}
