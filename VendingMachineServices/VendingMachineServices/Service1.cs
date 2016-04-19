using SomeLibraries;
using System.Linq;
using System.ServiceModel;

namespace VendingMachineService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Single)]
    public class Service1 : IService1
    {
        /// <summary>
        /// Кошелек пользователя
        /// </summary>
        Wallet _userWallet;
        /// <summary>
        /// Кошелек машины
        /// </summary>
        Wallet _machineWallet;
        /// <summary>
        /// Монетоприемник
        /// </summary>
        Wallet _cashWallet;
        /// <summary>
        /// Товарный состав машины
        /// </summary>
        VendingMachineContent _machineContent;

        /// <summary>
        /// какая часть денег в монетоприемнике принадлежит пользователю
        /// пример: там 10 руб + 5 руб, юзер купил чай за 13, сл-но из 15 руб в монетоприемнике, только 2ру всё ещё принадлежат покупателю
        /// </summary>
        uint _usersMoneyInCash;

        public DataContracts.CoinPackDataContract[] GetUserWallet()
        {
            return _userWallet.Select(p => new DataContracts.CoinPackDataContract(p.Nominal, p.Quantity)).ToArray();

        }

        public DataContracts.ItemTypeDataContract[] GetVendingMachineContent()
        {
            return _machineContent.Select(p => new DataContracts.ItemTypeDataContract(p.Id, p.Name, p.Price, p.Quantity)).ToArray();

        }

        public DataContracts.CoinPackDataContract[] GetVendingMachineWallet()
        {
            return _machineWallet.Select(p => new DataContracts.CoinPackDataContract(p.Nominal, p.Quantity)).ToArray();

        }

        public uint GetUsersMoneyInCash()
        {
            return _usersMoneyInCash;

        }

        /// <summary>
        /// Перемещает монету из кошелька пользователя в монетоприёмник
        /// </summary>
        /// <param name="nominal"></param>
        public void MoveCoinInCash(uint nominal)
        {
            try
            {
                _userWallet.Remove(nominal, 1);
                _cashWallet.Add(nominal, 1);
                _usersMoneyInCash += nominal;

            }
            catch
            {
                //TODO: откат атомарно

                throw;

            }

        }

        public void GetChange()
        {
            try
            {
                // вначале попробуем посчитать сдачу, вдруг да не получится набрать
                var change = _machineWallet.CalculateChange<LessCoinsAlgorithm>(_usersMoneyInCash);

                // все монеты из монетоприемника помещаются в кошель машины
                _machineWallet.Add(_cashWallet);
                _cashWallet.Clear();

                // сдача перемещается из кошеля машины в кошель пользователя
                _machineWallet.Remove(change);
                _usersMoneyInCash = 0;
                _userWallet.Add(change);

            }
            catch
            {
                //TODO: откат атомарно

                throw;

            }

        }

        public void BuyOne(int id)
        {
            Buy(id, 1);

        }

        public void Buy(int id, uint quantity)
        {
            var itemInfo = _machineContent.GetItemInfo(id);

            if (itemInfo.Quantity < quantity)
            {
                throw new NotEnoughItemsException();

            }

            if (itemInfo.Price * quantity > _usersMoneyInCash)
            {
                throw new NotEnoughMoneyException();

            }

            try
            {
                _machineContent.Remove(id, quantity);
                _usersMoneyInCash -= itemInfo.Price * quantity;

            }
            catch
            {
                //TODO: откат атомарно

                throw;

            }

        }

        public void Initialize(VendingMachineContent content, Wallet machineWallet, Wallet userWallet)
        {
            _machineContent = content;
            _machineWallet = machineWallet;
            _userWallet = userWallet;
            _cashWallet = new Wallet();

        }
    
    }

    public class NotEnoughMoneyException : KnownException
    { }

    public class NotEnoughItemsException : KnownException
    { }

}
