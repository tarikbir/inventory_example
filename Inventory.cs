using Newtonsoft.Json;
using System.Collections;
using System.Text;

namespace inventory_example.InventorySystem
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Inventory : IEnumerable<Item>, IDisposable
    {
        [JsonProperty("maximumCapacity")]
        public int MaximumCapacity = 1;

        [JsonProperty("inventory")]
        private Item[] m_inventory;

        [JsonProperty("name")]
        private string m_debugName;

        public Inventory(int maximumCapacity, string debugName = "inventory")
        {
            m_debugName = debugName;
            m_inventory = new Item[maximumCapacity];

            MaximumCapacity = maximumCapacity;
        }

        /// <summary>For usage with serialization</summary>
        [JsonConstructor]
        public Inventory(int maximumCapacity, string debugName, Item[] inventory) : this(maximumCapacity, debugName)
        {
            foreach (var item in inventory)
            {
                if (item != null && inventory[item.CurrentSlot] == item)
                {
                    Logger.Log($"Loaded {item} at {item.CurrentSlot}.");
                    SetItem(item, item.CurrentSlot);
                }
            }
        }

        public int Count => m_inventory.Length;

        public Item this[int index] { get => m_inventory[index]; private set => m_inventory[index] = value; }

        /// <summary>Moves an item to an inventory slot.</summary>
        public static bool Move(Item item, int newSlot, Inventory newInventory)
        {
            if (item == null || newInventory == null || newInventory.Count < newSlot) return false;
            if (item.CurrentSlot == newSlot && item.Context == newInventory) return true;
            if (newSlot < 0 || newInventory[newSlot] == null || newInventory[newSlot].IsStackableWith(item.ID))
            {
                item.Context?.Destroy(item, false);
                bool moveSuccess = newInventory.Add(item, newSlot);
                return moveSuccess;
            }
            else
            {
                return Swap(item, newInventory[newSlot]);
            }
        }

        /// <summary>Swaps items. Use Move() unless this swap call is absolutely necessary.</summary>
        public static bool Swap(Item fromItem, Item toItem)
        {
            if (fromItem == null || toItem == null || fromItem.Context == null || toItem.Context == null) return false;
            Inventory from = fromItem.Context;
            Inventory to = toItem.Context;
            int fromSlot = fromItem.CurrentSlot;
            int toSlot = toItem.CurrentSlot;
            to.Destroy(toItem, false);
            from.Destroy(fromItem, false);
            return to.Add(fromItem, toSlot) & from.Add(toItem, fromSlot); //TODO: If either fails, should roll back.
        }

        /// <summary>Moves an item to a new slot in this inventory.</summary>
        public bool Move(Item item, int newSlot)
        {
            return Move(item, newSlot, this);
        }

        /// <summary>Adds an item to this inventory.</summary>
        /// <param name="item">Added item.</param>
        /// <param name="slot">If given, tries to insert the item to slot.</param>
        public bool Add(Item item, int slot = -1)
        {
            if (slot < 0)
            {
                slot = GetFirstAvailableSlot(item);
            }

            if (slot >= 0) //Slot found.
            {
                PlaceItemIntoSlot(item, slot);
                return true;
            }
            else //No available slot or inventory full
            {
                return false;
            }
        }

        /// <summary>Allocates a new inventory, clearing item slots.</summary>
        /// <param name="disposeItems">If false, does not dispose of items.</param>
        public void Clear(bool disposeItems = true)
        {
            if (disposeItems)
            {
                Dispose();
            }

            m_inventory = new Item[MaximumCapacity];
        }

        /// <summary>Returns true if given item is in this inventory.</summary>
        /// <param name="item">Item.</param>
        public bool Contains(Item item)
        {
            return FindFirst(i => item == i) >= 0;
        }

        /// <summary>Returns true if given item id is in this inventory.</summary>
        /// <param name="id">Item ID.</param>
        public bool Contains(string id)
        {
            return FindFirst(i => i?.ID == id) >= 0;
        }

        /// <summary>Counts how many of an item is in this inventory.</summary>
        /// <param name="id">Item ID.</param>
        public int CountItem(string id)
        {
            int sum = 0;

            for (int i = 0; i < m_inventory.Length; i++)
            {
                if (m_inventory[i] == null) continue;
                if (m_inventory[i].ID == id) sum += m_inventory[i].Quantity;
            }

            return sum;
        }

        /// <summary>Counts items with specific tags in this inventory.</summary>
        /// <param name="tag">Tag.</param>
        public int CountTags(string tag)
        {
            int sum = 0;

            for (int i = 0; i < m_inventory.Length; i++)
            {
                if (m_inventory[i] == null) continue;
                if (m_inventory[i].Tags.Contains(tag) == true) sum += m_inventory[i].Quantity;
            }

            return sum;
        }

        /// <summary>Removes given amount from slot.</summary>
        /// <param name="slot">Inventory slot.</param>
        /// <param name="amount">Amount removed.</param>
        public void Remove(int slot, int amount)
        {
            if (amount > m_inventory[slot].Quantity) amount = m_inventory[slot].Quantity;
            Logger.Log($"Removing {amount} {m_inventory[slot]} at {slot}.");
            AddItem(slot, -amount);
        }

        /// <summary>Removes item with given ID.</summary>
        /// <param name="id">Item ID.</param>
        /// <param name="amount">Amount removed.</param>
        public void Remove(string id, int amount)
        {
            int slot = FindFirst(i => i != null && i.ID == id);
            if (slot < 0) return; //TODO: Error mechanism.
            Remove(slot, amount);
        }

        //TODO: Instead of searching constantly, may refer to the available slot directly after operations.
        /// <summary>Returns first available slot for item. Returns -1 if unable to find.</summary>
        /// <param name="item">Item.</param>
        private int GetFirstAvailableSlot(Item item)
        {
            return FindFirst(i => i == null || (i.IsStackableWith(item.ID) && !i.IsStackFull));
        }

        /// <summary>Sets item's inventory slot to null.</summary>
        /// <param name="item">Item.</param>
        /// <param name="disposeItem">If false, does not dispose of item.</param>
        public void Destroy(Item item, bool disposeItem = true)
        {
            Logger.Log($"Removing all {item} at {item.CurrentSlot}.");
            SetItem(null, item.CurrentSlot);
            if (disposeItem)
            {
                item.Dispose();
            }
        }

        /// <summary>Sets inventory slot to null.</summary>
        /// <param name="slot">Item slot.</param>
        /// <param name="disposeItem">If false, does not dispose of item.</param>
        public void Destroy(int slot, bool disposeItem = true)
        {
            Logger.Log($"Removing all {m_inventory[slot]} at {slot}.");
            if (disposeItem) m_inventory[slot].Dispose();
            else m_inventory[slot].SetEmptyContext();
            SetItem(null, slot);
        }

        public void Dispose()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (m_inventory[i] != null)
                {
                    Destroy(i, true);
                }
            }
        }

        #region Serialization
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Inventory? Deserialize(string jsonData)
        {
            return JsonConvert.DeserializeObject<Inventory>(jsonData);
        }
        #endregion

        #region AtomicMethods
        /// <summary>Decides if an item should be set or added to a slot.</summary>
        /// <param name="item">Item.</param>
        /// <param name="slot">Inventory slot.</param>
        /// <param name="sourceSlot">Optional. If given source, leftover items update the source.</param>
        private void PlaceItemIntoSlot(Item item, int slot = 0, int sourceSlot = -1)
        {
            if (m_inventory[slot] == null)
            {
                Logger.Log($"Placed {item} at {slot}.");
                SetItem(item, slot); //TODO: Logic needs to be checked for better firing event.
            }
            else
            {
                Logger.Log($"Adding {item} at {slot}.");
                AddItem(slot, item.Quantity, sourceSlot);
            }
        }

        /// <summary>Finds an item matching the predicate. Warning: Empty slots may throw an exception. Null checks are necessary.</summary>
        /// <param name="predicate">Predicate to run for all inventory.</param>
        /// <returns>Index of the first item matching the predicate.</returns>
        private int FindFirst(Func<Item, bool> predicate)
        {
            for (int i = 0; i < m_inventory.Length; i++)
            {
                if (predicate.Invoke(m_inventory[i])) return i;
            }

            return -1;
        }

        /// <summary>Sets the item in the slot.</summary>
        /// <param name="item">Item.</param>
        /// <param name="slot">Inventory slot. Can't be less than 0 or greater than max.</param>
        private void SetItem(Item? item, int slot)
        {
            m_inventory[slot] = item!;
            item?.SetContext(this, slot);
        }

        /// <summary>Adds item quantity in given slot.</summary>
        /// <param name="slot">Inventory slot.</param>
        /// <param name="amount">Amount to add to item quantity.</param>
        /// <param name="sourceSlot">Optional. If given source, leftover items update the source.</param>
        private void AddItem(int slot, int amount, int sourceSlot = -1)
        {
            int carryQuantity = m_inventory[slot].AddQuantity(amount);
            if (carryQuantity > 0) //Overflow
            {
                Add(m_inventory[slot].Copy(carryQuantity), sourceSlot); //TODO: move this logic out
            }

            if (m_inventory[slot].Quantity <= 0)
            {
                Destroy(m_inventory[slot]);
            }
        }
        #endregion

        private StringBuilder m_toStringSB = new();
        public override string ToString()
        {
            m_toStringSB.Clear();
            m_toStringSB.Append(m_debugName);
            m_toStringSB.Append(":\n");
            for (int i = 0; i < MaximumCapacity; i++)
            {
                m_toStringSB.Append('[');
                m_toStringSB.Append(m_inventory[i]?.ToString());
                m_toStringSB.Append("]\n");
            }
            return m_toStringSB.ToString();
        }

        public IEnumerator<Item> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                if (m_inventory[i] != null)
                    yield return m_inventory[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
