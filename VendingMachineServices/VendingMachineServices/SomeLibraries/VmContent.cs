using SomeLibraries;
using System.Collections.Generic;

namespace VendingMachineService
{
    public class VendingMachineContent : IEnumerable<VendingMachineContent.ItemType>
    {
        Dictionary<int, ItemType> _content = new Dictionary<int, ItemType>();

        public class ItemType
        {
            public int Id
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public uint Price
            {
                get;
                set;
            }

            public uint Quantity
            {
                get;
                set;
            }

            public ItemType Clone()
            {
                return new ItemType()
                {
                    Id = this.Id,
                    Name = this.Name,
                    Price = this.Price,
                    Quantity = this.Quantity
                };

            }

        }

        public void Clear()
        {
            _content.Clear();

        }

        public void Add(int id, string name, uint price, uint quantity)
        {
            if (!_content.ContainsKey(id))
            {
                _content.Add(id, new ItemType()
                {
                    Id = id,
                    Name = name,
                    Price = price,
                    Quantity = quantity
                });

            }
            else
            {
                // следует проверить наличие товара с тем же id, но отличными другими характеристиками
                if(_content[id].Name.Equals(name) && _content[id].Price.Equals(price))
                {
                    //TODO: переполн
                    _content[id].Quantity += quantity;

                }
                else
                {
                    throw new KnownException("В машине уже присутствует товар с указанным идентификатором, но отличными названием либо ценой");

                }

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <param name="id"></param>
        /// <returns></returns>
        public ItemType GetItemInfo(int id)
        {
            if(_content.ContainsKey(id))
            {
                return _content[id].Clone();

            }
            else
            {
                throw new ItemNotFoundException();

            }

        }

        public void Remove(int id, uint quantity)
        {
            if(_content.ContainsKey(id))
            {
                if(_content[id].Quantity < quantity)
                {
                    throw new NotEnoughItemsException();

                }

            }
            else
            {
                throw new ItemNotFoundException();

            }

            if(_content[id].Quantity == quantity)
            {
                _content.Remove(id);

            }
            else
            {
                _content[id].Quantity -= quantity;

            }

        }

        public IEnumerator<VendingMachineContent.ItemType> GetEnumerator()
        {
            return _content.Values.GetEnumerator();

        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _content.Values.GetEnumerator();

        }

        public class ItemNotFoundException : KnownException
        {
        }

    }

}
