using CoreLogic.Structures;
using CoreLogic.Vector;
using System;

namespace CoreLogic.Classes
{
    public class HashTable
    {
        // Внутренняя «ячейка» хэш‑таблицы
        public struct Hash
        {
            public Goods Value { get; set; }
            public Article Key { get; set; }
            public int Status { get; set; } // 0 = пусто, 1 = занято, 2 = удалено
        }

        private Hash[] _hashTable;
        private int _capacity;

        public HashTable(int capacity)
        {
            if (capacity < 1) throw new ArgumentException("Capacity must be > 0");
            _capacity = capacity;
            _hashTable = new Hash[_capacity];
            // все _hashTable[i].status == 0 по умолчанию
        }

        // Основная хэш‑функция по строковому представлению T
        private int HashFunction(Article key)
        {
            // используем key.ToString()
            ulong hash = 0;
            string s = key.ToString();
            for (int i = 0; i < s.Length; i++)
                hash += s[i];

            ulong sq = hash * hash;
            string sqs = sq.ToString();
            int mid = sqs.Length / 2;
            int start = mid - 2;
            if (start < 0) start = 0;
            int len = (sqs.Length - start >= 4 ? 4 : sqs.Length - start);
            string part = sqs.Substring(start, len);
            return int.Parse(part);
        }

        private int PrimaryHash(Article key) => HashFunction(key) % _capacity;

        private int SecondaryHash(int primary, int j)
        {
            int raw = primary + j * 1337 + j * j * 31;
            return Math.Abs(raw % _capacity);
        }

        /// <summary>
        /// Возвращает:
        ///  -1 если найден дубликат (по ключу),
        ///  -2 если таблица полна (но есть удалённые слоты),
        ///  иначе — индекс для вставки.
        /// </summary>
        private int ResolveConflict(int primaryIndex, Article key)
        {
            int firstDeleted = -1;
            int idx = primaryIndex;

            for (int j = 0; j < _capacity; j++)
            {
                ref var cell = ref _hashTable[idx];

                // Если ячейка пуста — вставляем сюда (или в первый удалённый)
                if (cell.Status == 0)
                {
                    return firstDeleted >= 0 ? firstDeleted : idx;
                }

                // Запоминаем первый встреченный удалённый слот
                if (cell.Status == 2 && firstDeleted < 0)
                {
                    firstDeleted = idx;
                }

                // Если ключи совпадают — дубликат
                if (cell.Status == 1 && cell.Key.Equals(key))
                {
                    return -1;
                }

                // Вторая проба
                idx = SecondaryHash(primaryIndex, j + 1);
            }

            // Таблица полна, но есть удалённые слоты
            return firstDeleted >= 0 ? firstDeleted : -2;
        }


        public bool Add(Goods data)
        {
            if (_capacity == 0) return false;

            int primary = PrimaryHash(data.Article);
            ref var slot = ref _hashTable[primary];

            if (slot.Status == 0)
            {
                slot.Value = data;
                slot.Status = 1;
                slot.Key = data.Article;
                CheckTableForCompleteness();
                return true;
            }

            int idx = ResolveConflict(primary, data.Article);
            if (idx == -1)
                return false;   // дубликат
            if (idx == -2)
            {
                Rehashing(expand: true);
                return Add(data);
            }

            _hashTable[idx].Value = data;
            _hashTable[idx].Status = 1;
            _hashTable[idx].Key = data.Article;
            CheckTableForCompleteness();
            return true;
        }

        public bool Remove(Goods data)
        {
            if (_capacity == 0) return false;

            int primary = PrimaryHash(data.Article);
            int idx = primary;

            for (int j = 0; j < _capacity; j++)
            {
                ref var cell = ref _hashTable[idx];
                if (cell.Status == 0)
                    return false;
                if (cell.Status == 1 && cell.Value.Equals(data))
                {
                    cell.Status = 2;
                    CheckTableForCompleteness();
                    return true;
                }
                idx = SecondaryHash(primary, j + 1);
            }

            return false;
        }

        public bool Find(Article key, out Goods found, out int steps)
        {
            found = default;
            steps = 0;
            if (_capacity == 0) return false;

            int primary = PrimaryHash(key);
            int idx = primary;

            for (int j = 0; j < _capacity; j++)
            {
                steps++;
                ref var cell = ref _hashTable[idx];

                // пустая ячейка — ключа тут уж точно нет
                if (cell.Status == 0)
                    return false;

                // ключ найден
                if (cell.Status == 1 && cell.Key.Equals(key))
                {
                    found = cell.Value;
                    return true;
                }

                idx = SecondaryHash(primary, j + 1);
            }

            return false;
        }


        private int TableCompleteness()
        {
            if (_capacity == 0) return 0;
            int used = 0;
            for (int i = 0; i < _capacity; i++)
                if (_hashTable[i].Status == 1)
                    used++;
            return used * 100 / _capacity;
        }

        private void Rehashing(bool expand)
        {
            int oldCap = _capacity;
            int newCap = expand ? oldCap * 2 : Math.Max(1, oldCap / 2);

            var oldTable = _hashTable;
            _hashTable = new Hash[newCap];
            _capacity = newCap;

            for (int i = 0; i < oldCap; i++)
            {
                var cell = oldTable[i];
                if (cell.Status == 1)
                {
                    int primary = HashFunction(cell.Key) % _capacity;
                    int idx = primary;
                    for (int j = 0; ; j++)
                    {
                        if (_hashTable[idx].Status != 1)
                        {
                            _hashTable[idx] = cell;
                            break;
                        }
                        idx = SecondaryHash(primary, j + 1);
                    }
                }
            }
        }

        public void RemoveAll()
        {
            for (int i = 0; i < _capacity; i++)
                if (_hashTable[i].Status == 1)
                    _hashTable[i].Status = 2;
        }

        public bool CheckTableForCompleteness()
        {
            int pct = TableCompleteness();
            if (pct > 70) { Rehashing(expand: true); return true; }
            if (pct < 30) { Rehashing(expand: false); return true; }
            return false;
        }

        public void Print(int limit = -1, int start = 0)
        {
            if (start < 0 || start >= _capacity) return;
            if (limit < 0 || limit > _capacity) limit = _capacity;

            Console.WriteLine("INDEX\tSTATUS\tVALUE");
            for (int i = start; i < limit; i++)
            {
                var cell = _hashTable[i];
                Console.Write(i + "\t" + cell.Status + "\t");
                if (cell.Status == 1)
                    Console.WriteLine(cell.Value);
                else
                    Console.WriteLine("<empty or deleted>");
            }
        }

        public Vector<Hash> HashTableToVector()
        {
            Vector<Hash> vector = new Vector<Hash>();

            for (int i = 0; i < _capacity; i++)
            {
                vector.Add(_hashTable[i]);
            }

            return vector;
        }

        public int Capacity => _capacity;
    }
}
