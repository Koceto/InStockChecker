using System.Collections.Generic;

namespace InStockChecker.Models
{
    /*
     * Oneplus Nord Teardown Case JerryRigEverything
     */

    public class OPNTCResponseModel
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public Dictionary<string, Item> items { get; set; }
    }

    public class Item
    {
        public string name { get; set; }
        public int stock { get; set; }
    }
}