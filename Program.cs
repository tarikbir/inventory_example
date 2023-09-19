using inventory_example;
using inventory_example.InventorySystem;

var swordData = new
{
    id = "sword",
    name = "Sword",
    tags = "weapon,equip",
    description = "This is a powerful sword.",
    stackSize = 1,
    buyValue = 100
};

var appleData = new
{
    id = "apple",
    name = "Apple",
    tags = "usable",
    description = "Nom nom.",
    stackSize = 20,
    buyValue = 3
};

Item sword = new(swordData);
Item apple = new(appleData);

ObjectWithInventories someGuy1 = new();

someGuy1.Backpack.Add(sword);
Console.WriteLine(someGuy1.Backpack);

someGuy1.Backpack.Add(apple);
Console.WriteLine(someGuy1.Backpack);

apple.AddQuantity(10);
Console.WriteLine(someGuy1.Backpack);

someGuy1.Backpack.Move(apple, 3);
Console.WriteLine(someGuy1.Backpack);