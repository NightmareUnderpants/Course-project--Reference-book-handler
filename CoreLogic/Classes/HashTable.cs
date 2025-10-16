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

            public override string ToString()
            {
                return $"{Value} {Key} {Status}";
            }
        }

        private Hash[] _hashTable;
        private int _capacity;

        public HashTable(int capacity)
        {
            Console.WriteLine($"HashTable.Constructor: Creating hash table with capacity {capacity}");
            if (capacity < 1) throw new ArgumentException("Capacity must be > 0");
            _capacity = capacity;
            _hashTable = new Hash[_capacity];
            Console.WriteLine($"HashTable.Constructor: Hash table created with {_capacity} slots");
            // все _hashTable[i].status == 0 по умолчанию
        }

        // Основная хэш‑функция по строковому представлению T
        private int HashFunction(Article key)
        {
            Console.WriteLine($"{key} HashTable.HashFunction: Calculating hash for key");
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
            int result = int.Parse(part);
            Console.WriteLine($"{key} HashTable.HashFunction: Hash result: {result}");
            return result;
        }

        private int PrimaryHash(Article key)
        {
            int result = HashFunction(key) % _capacity;
            Console.WriteLine($"{key} HashTable.PrimaryHash: Primary hash index: {result}");
            return result;
        }

        private int SecondaryHash(int primary, int j)
        {
            int raw = primary + j * 1337 + j * j * 31;
            int result = Math.Abs(raw % _capacity);
            Console.WriteLine($"HashTable.SecondaryHash: Primary: {primary}, j: {j}, result: {result}");
            return result;
        }

        /// <summary>
        /// Возвращает:
        ///  -1 если найден дубликат (по ключу),
        ///  -2 если таблица полна (но есть удалённые слоты),
        ///  иначе — индекс для вставки.
        /// </summary>
        private int ResolveConflict(int primaryIndex, Article key)
        {
            Console.WriteLine($"{key} HashTable.ResolveConflict: Resolving conflict for primary index {primaryIndex}");
            int firstDeleted = -1;
            int idx = primaryIndex;

            for (int j = 0; j < _capacity; j++)
            {
                ref var cell = ref _hashTable[idx];
                Console.WriteLine($"{key} HashTable.ResolveConflict: Checking index {idx}, status: {cell.Status}");

                // Если ячейка пуста — вставляем сюда (или в первый удалённый)
                if (cell.Status == 0)
                {
                    Console.WriteLine($"{key} HashTable.ResolveConflict: Found empty slot at index {idx}, firstDeleted: {firstDeleted}");
                    return firstDeleted >= 0 ? firstDeleted : idx;
                }

                // Запоминаем первый встреченный удалённый слот
                if (cell.Status == 2 && firstDeleted < 0)
                {
                    firstDeleted = idx;
                    Console.WriteLine($"{key} HashTable.ResolveConflict: Found first deleted slot at index {idx}");
                }

                // Если ключи совпадают — дубликат
                if (cell.Status == 1 && cell.Key.Equals(key))
                {
                    Console.WriteLine($"{key} HashTable.ResolveConflict: Found duplicate key at index {idx}");
                    return -1;
                }

                // Вторая проба
                idx = SecondaryHash(primaryIndex, j + 1);
                Console.WriteLine($"{key} HashTable.ResolveConflict: Trying next index: {idx}");
            }

            Console.WriteLine($"{key} HashTable.ResolveConflict: Table full, firstDeleted: {firstDeleted}");
            // Таблица полна, но есть удалённые слоты
            return firstDeleted >= 0 ? firstDeleted : -2;
        }


        public bool Add(Goods data)
        {
            Console.WriteLine($"{data.Article} HashTable.Add: Starting add operation");
            if (_capacity == 0)
            {
                Console.WriteLine($"{data.Article} HashTable.Add: Capacity is 0, cannot add");
                return false;
            }

            int primary = PrimaryHash(data.Article);
            ref var slot = ref _hashTable[primary];
            Console.WriteLine($"{data.Article} HashTable.Add: Primary slot {primary}, status: {slot.Status}");

            if (slot.Status == 0)
            {
                Console.WriteLine($"{data.Article} HashTable.Add: Primary slot is empty, inserting directly");
                slot.Value = data;
                slot.Status = 1;
                slot.Key = data.Article;
                CheckTableForCompleteness();
                Console.WriteLine($"{data.Article} HashTable.Add: Successfully added to primary slot");
                return true;
            }

            Console.WriteLine($"{data.Article} HashTable.Add: Primary slot occupied, resolving conflict");
            int idx = ResolveConflict(primary, data.Article);
            Console.WriteLine($"{data.Article} HashTable.Add: Conflict resolution result: {idx}");

            if (idx == -1)
            {
                Console.WriteLine($"{data.Article} HashTable.Add: Duplicate found, cannot add");
                return false;   // дубликат
            }
            if (idx == -2)
            {
                Console.WriteLine($"{data.Article} HashTable.Add: Table full, rehashing and retrying");
                Rehashing(expand: true);
                return Add(data);
            }

            Console.WriteLine($"{data.Article} HashTable.Add: Inserting at resolved index {idx}");
            _hashTable[idx].Value = data;
            _hashTable[idx].Status = 1;
            _hashTable[idx].Key = data.Article;
            CheckTableForCompleteness();
            Console.WriteLine($"{data.Article} HashTable.Add: Successfully added at index {idx}");
            return true;
        }

        public bool Remove(Goods data)
        {
            Console.WriteLine($"{data.Article} HashTable.Remove: Starting remove operation");
            if (_capacity == 0)
            {
                Console.WriteLine($"{data.Article} HashTable.Remove: Capacity is 0, cannot remove");
                return false;
            }

            int primary = PrimaryHash(data.Article);
            int idx = primary;
            Console.WriteLine($"{data.Article} HashTable.Remove: Primary index: {primary}");

            for (int j = 0; j < _capacity; j++)
            {
                ref var cell = ref _hashTable[idx];
                Console.WriteLine($"{data.Article} HashTable.Remove: Checking index {idx}, status: {cell.Status}");

                if (cell.Status == 0)
                {
                    Console.WriteLine($"{data.Article} HashTable.Remove: Empty slot found, element not present");
                    return false;
                }
                if (cell.Status == 1 && cell.Value.Equals(data))
                {
                    Console.WriteLine($"{data.Article} HashTable.Remove: Found element at index {idx}, marking as deleted");
                    cell.Status = 2;
                    CheckTableForCompleteness();
                    Console.WriteLine($"{data.Article} HashTable.Remove: Successfully removed");
                    return true;
                }
                idx = SecondaryHash(primary, j + 1);
                Console.WriteLine($"{data.Article} HashTable.Remove: Trying next index: {idx}");
            }

            Console.WriteLine($"{data.Article} HashTable.Remove: Element not found after checking all slots");
            return false;
        }

        public bool Find(Article key, out Goods found, out int steps)
        {
            Console.WriteLine($"{key} HashTable.Find: Starting find operation");
            found = default;
            steps = 0;
            if (_capacity == 0)
            {
                Console.WriteLine($"{key} HashTable.Find: Capacity is 0, cannot find");
                return false;
            }

            int primary = PrimaryHash(key);
            int idx = primary;
            Console.WriteLine($"{key} HashTable.Find: Primary index: {primary}");

            for (int j = 0; j < _capacity; j++)
            {
                steps++;
                ref var cell = ref _hashTable[idx];
                Console.WriteLine($"{key} HashTable.Find: Step {steps}, checking index {idx}, status: {cell.Status}");

                // пустая ячейка — ключа тут уж точно нет
                if (cell.Status == 0)
                {
                    Console.WriteLine($"{key} HashTable.Find: Empty slot, key not found");
                    return false;
                }

                // ключ найден
                if (cell.Status == 1 && cell.Key.Equals(key))
                {
                    found = cell.Value;
                    Console.WriteLine($"{key} HashTable.Find: Key found at index {idx} after {steps} steps");
                    return true;
                }

                idx = SecondaryHash(primary, j + 1);
                Console.WriteLine($"{key} HashTable.Find: Trying next index: {idx}");
            }

            Console.WriteLine($"{key} HashTable.Find: Key not found after checking all slots");
            return false;
        }


        private int TableCompleteness()
        {
            Console.WriteLine($"HashTable.TableCompleteness: Calculating table completeness");
            if (_capacity == 0)
            {
                Console.WriteLine($"HashTable.TableCompleteness: Capacity is 0, completeness 0%");
                return 0;
            }
            int used = 0;
            for (int i = 0; i < _capacity; i++)
                if (_hashTable[i].Status == 1)
                    used++;
            var capacity = used * 100 / _capacity;
            Console.WriteLine($"HashTable.TableCompleteness: {used}/{_capacity} slots used ({capacity}%)");
            return capacity;
        }

        private void Rehashing(bool expand)
        {
            Console.WriteLine($"HashTable.Rehashing: Starting rehashing, expand: {expand}");
            int oldCap = _capacity;
            int newCap = expand ? oldCap * 2 : Math.Max(1, oldCap / 2);
            Console.WriteLine($"HashTable.Rehashing: Old capacity: {oldCap}, new capacity: {newCap}");

            var oldTable = _hashTable;
            _hashTable = new Hash[newCap];
            _capacity = newCap;
            Console.WriteLine($"HashTable.Rehashing: New table created with {newCap} slots");

            int migrated = 0;
            for (int i = 0; i < oldCap; i++)
            {
                var cell = oldTable[i];
                if (cell.Status == 1)
                {
                    Console.WriteLine($"{cell.Key} HashTable.Rehashing: Migrating element from old index {i}");
                    int primary = HashFunction(cell.Key) % _capacity;
                    int idx = primary;
                    for (int j = 0; ; j++)
                    {
                        if (_hashTable[idx].Status != 1)
                        {
                            _hashTable[idx] = cell;
                            migrated++;
                            Console.WriteLine($"{cell.Key} HashTable.Rehashing: Migrated to new index {idx}");
                            break;
                        }
                        idx = SecondaryHash(primary, j + 1);
                    }
                }
            }
            Console.WriteLine($"HashTable.Rehashing: Completed, migrated {migrated} elements");
        }

        public void RemoveAll()
        {
            Console.WriteLine($"HashTable.RemoveAll: Starting remove all operation");
            int removed = 0;
            for (int i = 0; i < _capacity; i++)
            {
                if (_hashTable[i].Status == 1)
                {
                    _hashTable[i].Status = 2;
                    removed++;
                    Console.WriteLine($"{_hashTable[i].Key} HashTable.RemoveAll: Marked slot {i} as deleted");
                }
            }
            Console.WriteLine($"HashTable.RemoveAll: Completed, removed {removed} elements");
        }

        public bool CheckTableForCompleteness()
        {
            Console.WriteLine($"HashTable.CheckTableForCompleteness: Checking table completeness");
            int pct = TableCompleteness();
            Console.WriteLine($"HashTable.CheckTableForCompleteness: Current completeness: {pct}%");

            if (pct > 70)
            {
                Console.WriteLine($"HashTable.CheckTableForCompleteness: Completeness > 70%, expanding table");
                Rehashing(expand: true);
                return true;
            }
            if (pct < 30)
            {
                Console.WriteLine($"HashTable.CheckTableForCompleteness: Completeness < 30%, shrinking table");
                Rehashing(expand: false);
                return true;
            }
            Console.WriteLine($"HashTable.CheckTableForCompleteness: Completeness within acceptable range (30-70%)");
            return false;
        }

        public void Print(int limit = -1, int start = 0)
        {
            Console.WriteLine($"HashTable.Print: Printing table, start: {start}, limit: {limit}");
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
            Console.WriteLine($"HashTable.Print: Printed {limit - start} slots");
        }

        public Vector<Hash> HashTableToVector()
        {
            Console.WriteLine($"HashTable.HashTableToVector: Converting hash table to vector");
            Vector<Hash> vector = new Vector<Hash>();

            for (int i = 0; i < _capacity; i++)
            {
                vector.Add(_hashTable[i]);
                Console.WriteLine($"{_hashTable[i].Key} HashTable.HashTableToVector: Added slot {i} to vector");
            }

            Console.WriteLine($"HashTable.HashTableToVector: Converted {_capacity} slots to vector");
            return vector;
        }

        public int Capacity
        {
            get
            {
                Console.WriteLine($"HashTable.Capacity: Returning capacity: {_capacity}");
                return _capacity;
            }
        }
    }
}