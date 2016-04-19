using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachineService.DataContracts
{
    [DataContract]
    public class CoinPackDataContract
    {
        [DataMember]
        public uint Nominal
        {
            get;
            set;
        }

        [DataMember]
        public uint Quantity
        {
            get;
            set;
        }

        public CoinPackDataContract(uint nominal, uint quantity)
        {
            Nominal = nominal;
            Quantity = quantity;

        }

    }

    [DataContract]
    public class ItemTypeDataContract
    {
        [DataMember]
        public int Id
        {
            get;
            set;
        }

        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
        public uint Price
        {
            get;
            set;
        }

        [DataMember]
        public uint Quantity
        {
            get;
            set;
        }

        public ItemTypeDataContract(int id, string name, uint price, uint quantity)
        {
            Id = id;
            Name = name;
            Price = price;
            Quantity = quantity;

        }

    }

}
