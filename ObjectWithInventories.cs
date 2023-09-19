
using inventory_example.InventorySystem;

namespace inventory_example
{
    public class ObjectWithInventories
    {
        public Inventory Backpack;
        public Inventory Equipment;

        public ObjectWithInventories()
        {
            Backpack = new(10, "backpack");
            Equipment = new(4, "equips");
        }
    }
}
