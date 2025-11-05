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
            public CircularLinkedList<Goods>.Node NodeRef { get; set; } // Ссылка на узел списка
            public Article Key { get; set; }
            public int Status { get; set; } // 0 = пусто, 1 = занято, 2 = удалено

            public override string ToString()
            {
                return $"{NodeRef?.Value} {Key} {Status}";
            }
        }

        private Hash[] _hashTable;
        private int _capacity;

        public HashTable(int capacity)
        {
            if (capacity < 1) throw new ArgumentException("Capacity must be > 0");

            _capacity = capacity;
            _hashTable = new Hash[_capacity];
        }

        // Основная хэш‑функция
        private int HashFunction(Article key)
        {
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

        private int PrimaryHash(Article key)
        {
            return HashFunction(key) % _capacity;
        }

        private int SecondaryHash(int primary, int j)
        {
            int raw = primary + j * 1337 + j * j * 31;
            return Math.Abs(raw % _capacity);
        }

        private int ResolveConflict(int primaryIndex, Article key)
        {
            int firstDeleted = -1;
            int idx = primaryIndex;

            for (int j = 0; j < _capacity; j++)
            {
                ref var cell = ref _hashTable[idx];

                if (cell.Status == 0)
                    return firstDeleted >= 0 ? firstDeleted : idx;

                if (cell.Status == 2 && firstDeleted < 0)
                    firstDeleted = idx;

                if (cell.Status == 1 && cell.Key.Equals(key))
                    return -1;

                idx = SecondaryHash(primaryIndex, j + 1);
            }

            return firstDeleted >= 0 ? firstDeleted : -2;
        }

        /// <summary>
        /// Добавляет товар в хеш-таблицу, сохраняя ссылку на узел в общем списке
        /// </summary>
        public bool Add(Article key, CircularLinkedList<Goods>.Node nodeRef)
        {
            if (_capacity == 0 || nodeRef == null)
                return false;

            int primary = PrimaryHash(key);
            ref var slot = ref _hashTable[primary];

            if (slot.Status == 0)
            {
                slot.NodeRef = nodeRef;
                slot.Key = key;
                slot.Status = 1;
                CheckTableForCompleteness();
                return true;
            }

            int idx = ResolveConflict(primary, key);
            if (idx == -1) return false; // Дубликат
            if (idx == -2)
            {
                Rehashing(expand: true);
                return Add(key, nodeRef); // Рекурсивный вызов после расширения
            }

            _hashTable[idx].NodeRef = nodeRef;
            _hashTable[idx].Key = key;
            _hashTable[idx].Status = 1;
            CheckTableForCompleteness();
            return true;
        }

        public bool Remove(Article key)
        {
            if (_capacity == 0) return false;

            int primary = PrimaryHash(key);
            int idx = primary;

            for (int j = 0; j < _capacity; j++)
            {
                ref var cell = ref _hashTable[idx];
                if (cell.Status == 0) return false;
                if (cell.Status == 1 && cell.Key.Equals(key))
                {
                    cell.Status = 2; // Помечаем как удалённый
                    CheckTableForCompleteness();
                    return true;
                }
                idx = SecondaryHash(primary, j + 1);
            }
            return false;
        }

        public bool Find(Article key, out CircularLinkedList<Goods>.Node foundNode)
        {
            foundNode = null;
            if (_capacity == 0) return false;

            int primary = PrimaryHash(key);
            int idx = primary;

            for (int j = 0; j < _capacity; j++)
            {
                ref var cell = ref _hashTable[idx];
                if (cell.Status == 0) return false;
                if (cell.Status == 1 && cell.Key.Equals(key))
                {
                    foundNode = cell.NodeRef;
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

            // Миграция существующих элементов
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

        public bool CheckTableForCompleteness()
        {
            int pct = TableCompleteness();
            if (pct > 70)
            {
                Rehashing(expand: true);
                return true;
            }
            if (pct < 30)
            {
                Rehashing(expand: false);
                return true;
            }
            return false;
        }

        public int Capacity => _capacity;

        public Vector<Hash> HashTableToVector()
        {
            Vector<Hash> vector = new Vector<Hash>();

            for (int i = 0; i < _capacity; i++)
            {
                vector.Add(_hashTable[i]);
            }
            
            return vector;
        }

    }
}