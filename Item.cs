using Newtonsoft.Json;

namespace inventory_example.InventorySystem
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Item : IDisposable, ISlotContainer
    {
        #region UI Properties
        [JsonProperty("id")]
        public string ID { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; private set; }
        
        [JsonProperty("description")]
        public string Description { get; private set; }
        #endregion

        #region Mechanics
        [JsonProperty("quantity")]
        public int Quantity { get; private set; }

        [field: NonSerialized]
        public Inventory? Context { get; private set; }

        [JsonProperty("currentSlot")]
        public int CurrentSlot { get; private set; }

        public int StackSize { get; private set; }

        [JsonProperty("baseSell")]
        public int BaseSellPrice { get; private set; }

        [JsonProperty("baseBuy")]
        public int BaseBuyPrice { get; private set; }
        #endregion

        public string Icon { get => m_data.Icon; }

        [JsonProperty("guid")]
        private readonly string m_guid;
        protected dynamic m_data;
        protected bool m_disposedValue;

        public bool IsStackableWith(string itemID) => ID == itemID;

        public Item(dynamic data, int quantity = 1, Inventory? context = null, int currentSlot = 0, string guid = default)
        {
            //Set Members
            m_data = data;
            m_guid = guid ?? Guid.NewGuid().ToString();

            //Set Properties
            ID = m_data.id;
            Name = m_data.name;
            Tags = (m_data.tags as string).Split(',').ToList();
            Description = m_data.description;
            StackSize = m_data.stackSize;
            BaseBuyPrice = m_data.buyValue * 2;
            BaseSellPrice = m_data.buyValue;

            //Set Instance Properties
            Context = context;
            Quantity = quantity;
            CurrentSlot = currentSlot;
        }

        public bool IsStackFull => Quantity >= m_data.StackSize;

        /// <summary>Adds amount to item stack. Returns carry, if there's overflow. Also returns negative when there's not enough to consume. Does not automatically set to 0 when negative.</summary>
        /// <param name="amount">Amount to add.</param>
        public int AddQuantity(int amount)
        {
            int newQuantity = Quantity + amount;
            if (newQuantity < 0) return newQuantity;
            Quantity = Math.Min(StackSize, newQuantity);
            return newQuantity - Quantity;
        }

        /// <summary>Splits an item stack by amount. Doesn't split by quantity (it should move instead). Returns the remaining item.</summary>
        public Item? SplitBy(int amount)
        {
            if (Quantity < 2 || amount >= Quantity) return null;
            int newItemAmount = Quantity - amount;
            AddQuantity(-amount);
            return Copy(newItemAmount);
        }

        /// <summary>Splits an item stack in half. Returns the remaining item.</summary>
        public Item? SplitHalf()
        {
            if (Quantity < 2) return null;
            int removedAmount = Quantity / 2;
            int newItemAmount = Quantity - removedAmount;
            AddQuantity(-removedAmount);
            return Copy(newItemAmount);
        }

        /// <summary>Returns a detached no-context item. Must call SetInventoryContext next.</summary>
        /// <param name="quantity"></param>
        public virtual Item Copy(int quantity = 0)
        {
            return new(m_data, quantity == 0 ? Quantity : quantity);
        }

        public void SetEmptyContext()
        {
            Context = null;
            CurrentSlot = -1;
        }

        public void SetContext(Inventory context, int slot)
        {
            Context = context;
            CurrentSlot = slot;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposedValue)
            {
                if (disposing)
                {
                    //Dispose managed objects
                    m_data = null;
                    Tags = null;
                    Context = null;
                }

                //Free unmanaged objects
                m_disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return $"[{Quantity} {Name}] (H: <i>{m_guid}</i>)";
        }
    }
}
